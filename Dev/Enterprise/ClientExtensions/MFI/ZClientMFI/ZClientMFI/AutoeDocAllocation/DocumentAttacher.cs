using System;
using System.IO;
using System.Text.RegularExpressions;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.DocumentScanning;
using Enterprise.DocumentScanning.Business;
using Enterprise.Freight.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.Freight.Forwarding.Orders.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Client.MFI.AutoeDocAllocation
{
	public class DocumentAttacher
	{
		public bool AttachFile(FileInfo documentToAttach)
		{
			bool successfullyAttached = true;

			string fileName = documentToAttach.Name.ToUpper();
			string documentReference = GetDocumentRef(fileName);
			DetermineBusinessDocumentAndReferenceType(fileName);
			try
			{
				var masterFactory = new DocumentFactoryProvider().GetFactory(new BusinessObjectFactory());
				successfullyAttached = AttachFileDocument(masterFactory, documentToAttach, documentReference);
				masterFactory.Save();
			}
			catch (Exception e) when (!e.IsCriticalException())
			{
				successfullyAttached = false;
			}

			return successfullyAttached;
		}

		#region Implementation

		bool AttachFileDocument(DocumentFactory masterFactory, FileInfo file, ZString docRefCode)
		{
			bool fileAttached = false;
			var refPK = FindObjectRefPK(masterFactory, docRefCode);
			if (!refPK.IsEmpty)
			{
				string docoType = DocType.ToString();
				byte[] contents = DocumentUtilities.GetFileAsBytes(file.FullName);

				fileAttached = ImportFileFromFileSystem(masterFactory, contents, file.Name, BusinessRefType, refPK, docoType, "");
			}

			if (fileAttached && file.Name.ToUpper().StartsWith("TLX."))
			{
				UpdateShipmentReleaseType(refPK);
			}

			return fileAttached;
		}

		void UpdateShipmentReleaseType(ZGuid shipmentPK)
		{
			var factory = new BusinessObjectFactory();
			var shipment = factory.Load<ForwardingShipment>(shipmentPK);
			if (shipment != null)
			{
				shipment.JS_ReleaseType = "EBL";
				factory.Save();
			}
		}

		ZGuid FindObjectRefPK(DocumentFactory masterFactory, ZString docRefCode)
		{
			var objRefPK = ZGuid.Empty;
			if (DocRefType == MFIConstants.AutoeDoc.DocRefType.Masterbill ||
				 DocRefType == MFIConstants.AutoeDoc.DocRefType.Housebill ||
				 DocRefType == MFIConstants.AutoeDoc.DocRefType.Container)
			{
				objRefPK = GetConsolShipmentOrDecPKFromMawbHawbOrContainer(docRefCode);
			}
			else if (DocRefType == MFIConstants.AutoeDoc.DocRefType.Order)
			{
				objRefPK = GetOrderPKFromBuyerOrderNum(docRefCode);
			}
			else if (DocRefType == MFIConstants.AutoeDoc.DocRefType.ConNotePOD)
			{
				objRefPK = GetShipmentOrDecPKFromConNote(docRefCode);
			}
			else
			{
				objRefPK = AssemblyDataLookup.GetPKFromCode(masterFactory, BusinessRefType, docRefCode, false);
				if (objRefPK == ZGuid.Empty && (AttachDocumentTo == AttachTo.ConsolOrDec || AttachDocumentTo == AttachTo.ShipmentOrDec))
				{
					objRefPK = AssemblyDataLookup.GetPKFromCode(masterFactory, MFIConstants.AutoeDoc.BusinessRefType.Declaration, docRefCode, false);
				}
			}

			return objRefPK;
		}

		ZGuid GetConsolShipmentOrDecPKFromMawbHawbOrContainer(ZString mawbHawbContainer)
		{
			var objRefPK = ZGuid.Empty;
			var factory = new BusinessObjectFactory();

			if (DocRefType == MFIConstants.AutoeDoc.DocRefType.Masterbill)
			{
				var consolFilter = new ZQuery(JobConsolSchema.JK_MasterBillNum, mawbHawbContainer);
				var consol = factory.LoadTop1<CommonConsol>(consolFilter);

				if (consol != null)
				{
					objRefPK = consol.PK;
				}
				else if (AttachDocumentTo == AttachTo.ConsolOrDec || AttachDocumentTo == AttachTo.Brokerage)
				{
					objRefPK = CheckDeclarationForRef(factory, JobDeclarationSchema.JE_MasterBill, mawbHawbContainer);
				}
			}
			else if (DocRefType == MFIConstants.AutoeDoc.DocRefType.Housebill)
			{
				var filter = new ZQuery(JobShipmentSchema.JS_HouseBill, mawbHawbContainer);
				var jobShipment = factory.LoadTop1<ForwardingShipment>(filter);

				if (jobShipment != null)
				{
					objRefPK = jobShipment.PK;
				}
				else if (AttachDocumentTo == AttachTo.ShipmentOrDec)
				{
					objRefPK = CheckDeclarationForRef(factory, JobDeclarationSchema.JE_HouseBill, mawbHawbContainer);
				}
			}
			else if (DocRefType == MFIConstants.AutoeDoc.DocRefType.Container)
			{
				var jobContainerFilter = new ZQuery(JobContainerSchema.JC_ContainerNum, mawbHawbContainer);
				var jobContainer = factory.LoadTop1<CommonContainer>(jobContainerFilter);

				if (jobContainer != null)
				{
					objRefPK = jobContainer.JC_JK;
				}
				else
				{
					var cusContainerFilter = new ZQuery(CusContainerSchema.CO_ContainerNumber, mawbHawbContainer);
					var decContainer = factory.LoadTop1<BaseCusContainer>(cusContainerFilter);

					if (decContainer != null)
					{
						BusinessRefType = MFIConstants.AutoeDoc.BusinessRefType.Declaration;
						objRefPK = decContainer.CO_JE;
					}
				}
			}

			return objRefPK;
		}

		ZGuid GetShipmentOrDecPKFromConNote(ZString conNote)
		{
			var objRefPK = ZGuid.Empty;
			var factory = new BusinessObjectFactory();
			var shipmentFilter = new ZQuery(JobDocsAndCartageSchema.JP_CustomAttrib1, conNote);
			shipmentFilter.AddToFilter(JobDocsAndCartageSchema.JP_ParentTableCode, "JS");
			var docsAndCartage = factory.LoadTop1<JobDocsAndCartage>(shipmentFilter);

			if (docsAndCartage == null)
			{
				var declarationFilter = new ZQuery(JobDocsAndCartageSchema.JP_CustomAttrib1, conNote);
				declarationFilter.AddToFilter(JobDocsAndCartageSchema.JP_ParentTableCode, "JE");
				docsAndCartage = factory.LoadTop1<JobDocsAndCartage>(declarationFilter);
			}

			if (docsAndCartage != null)
			{
				objRefPK = docsAndCartage.JP_ParentID;
			}

			return objRefPK;
		}

		ZGuid CheckDeclarationForRef(BusinessObjectFactory factory, SchemaStringColumn fieldToCheck, string reference)
		{
			var decPK = ZGuid.Empty;

			var declarationFilter = new ZQuery(fieldToCheck, reference);
			var cusDec = factory.LoadTop1<BaseJobDeclaration>(declarationFilter);
			if (cusDec != null)
			{
				BusinessRefType = MFIConstants.AutoeDoc.BusinessRefType.Declaration;
				decPK = cusDec.PK;
			}

			return decPK;
		}

		ZGuid GetOrderPKFromBuyerOrderNum(ZString buyerOrderNumber)
		{
			var objRefPK = ZGuid.Empty;
			var factory = new BusinessObjectFactory();
			int refSeparator = buyerOrderNumber.IndexOf(".");
			string buyer = buyerOrderNumber.SubstringSafe(0, refSeparator);
			string orderNumber = buyerOrderNumber.SubstringSafe(refSeparator + 1);
			var buyerOrg = factory.LoadFromNaturalKey<OrgHeader>(OrgHeaderSchema.OH_Code, buyer);

			if (buyerOrg != null)
			{
				var orderFilter = new ZQuery(JobOrderHeaderSchema.JD_OA_BuyerAddress, buyerOrg.Addresses.Select(x => x.PK));
				orderFilter.AddToFilter(JobOrderHeaderSchema.JD_OrderNumber, orderNumber);
				var jobOrder = factory.LoadTop1<Order>(orderFilter);

				if (jobOrder != null)
				{
					objRefPK = jobOrder.PK;
				}
			}

			return objRefPK;
		}

		bool ImportFileFromFileSystem(DocumentFactory masterFactory, byte[] contents, ZString filenameOnly, ZString userSuppliedRefType, ZGuid userSuppliedRefPK, ZString userSuppliedDocType, ZString userSuppliedDocSource)
		{
			byte[] contentsToImport = contents;
			string filenameOnlyToImport = StripDateTimeStringFromFileName(filenameOnly);

			if (FileImporter.IsSupportedImageFile(Path.GetExtension(filenameOnly)))
			{
				contentsToImport = DocumentUtilities.ConvertFileToTiff(contents, filenameOnly);
				filenameOnlyToImport = Path.GetFileNameWithoutExtension(filenameOnly) + ".TIF";
			}

			return masterFactory.Import(contentsToImport, filenameOnlyToImport, userSuppliedRefType, userSuppliedRefPK, userSuppliedDocType, userSuppliedDocSource, ZGuid.Empty, ZGuid.Empty, ZGuid.Empty);
		}

		public
		ZString GetDocumentRef(string fileName)
		{
			int fileNameFirstSeparator = fileName.IndexOf(".");
			string unstrungFileName = fileName.Substring(fileNameFirstSeparator + 1);
			int fileNameSecondSeparator = unstrungFileName.IndexOf(".");

			if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_ORD))
			{
				string orderBuyer = unstrungFileName.Substring(0, fileNameSecondSeparator);
				string fileNameRemainder = unstrungFileName.Substring(fileNameSecondSeparator + 1);
				int fileNameThirdSeparator = fileNameRemainder.IndexOf(".");
				string orderNumber = fileNameRemainder.Substring(0, fileNameThirdSeparator);
				unstrungFileName = orderBuyer + "." + orderNumber;
			}
			else
			{
				unstrungFileName = unstrungFileName.Substring(0, fileNameSecondSeparator);
			}

			return unstrungFileName;
		}

		static Regex UnderscoreDateTimeRegex
		{
			get { return underscoreDateTimeRegex ?? (underscoreDateTimeRegex = new Regex("_[1-2][0-9][0-9][0-9]+", RegexOptions.Compiled)); }
		}
		[ThreadStatic]
		static Regex underscoreDateTimeRegex;

		string StripDateTimeStringFromFileName(string fileName)
		{
			string readableFileName = fileName;

			var match = UnderscoreDateTimeRegex.Match(fileName);

			if (match.Index > 0)
			{
				readableFileName = fileName.Remove(match.Index);
			}

			return readableFileName;
		}

		enum AttachTo { Organisation, Consol, Shipment, Brokerage, Order, Booking, ConsolOrDec, ShipmentOrDec, Unknown }
		AttachTo AttachDocumentTo;
		string BusinessRefType;
		enum DocumentType { AGI, COM, EBL, HBL, OBL, ORG, USA, POD, UKN }
		DocumentType DocType;
		ZString DocRefType = ZString.Empty;

		#region DetermineBusinessDocumentAndReferenceType

		void DetermineBusinessDocumentAndReferenceType(string fileName)
		{
			AttachDocumentTo = GetBusinessType(fileName);
			DocType = GetDocumentType(fileName);
			DocRefType = GetDocumentReferenceType(fileName);
		}

		#region GetBusinessType

		AttachTo GetBusinessType(string fileName)
		{
			if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_ORD))
			{
				BusinessRefType = MFIConstants.AutoeDoc.BusinessRefType.Order;
				return AttachTo.Order;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.Various_ORG))
			{
				BusinessRefType = MFIConstants.AutoeDoc.BusinessRefType.Organisation;
				return AttachTo.Organisation;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.OceanBills_OBL) ||
						fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.AgentAccountNotes_AGI))
			{
				BusinessRefType = MFIConstants.AutoeDoc.BusinessRefType.Consol;
				return AttachTo.ConsolOrDec;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.AgentAccountNotes_C) ||
						fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.OceanBills_O))
			{
				BusinessRefType = MFIConstants.AutoeDoc.BusinessRefType.Consol;
				return AttachTo.Consol;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_BOO))
			{
				BusinessRefType = MFIConstants.AutoeDoc.BusinessRefType.Booking;
				return AttachTo.Booking;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_B) ||
						fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.OceanBills_BO) ||
						fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_COMOBL))
			{
				BusinessRefType = MFIConstants.AutoeDoc.BusinessRefType.Declaration;
				return AttachTo.Brokerage;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.HouseBills_BL) ||
						fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_COM) ||
						fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_S))
			{
				BusinessRefType = MFIConstants.AutoeDoc.BusinessRefType.Shipment;
				return AttachTo.ShipmentOrDec;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.TransportConNote_TSP))
			{
				BusinessRefType = MFIConstants.AutoeDoc.BusinessRefType.ConNote;
				return AttachTo.ShipmentOrDec;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.ExpressHouseBill_TLX))
			{
				BusinessRefType = MFIConstants.AutoeDoc.BusinessRefType.Shipment;
				return AttachTo.ShipmentOrDec;
			}
			else
			{
				BusinessRefType = "";
				return AttachTo.Unknown;
			}
		}

		#endregion

		#region GetDocumentType

		DocumentType GetDocumentType(string fileName)
		{
			if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.Various_ORG))
			{
				return DocumentType.ORG;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.AgentAccountNotes_AGI) ||
						fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.AgentAccountNotes_C))
			{
				return DocumentType.AGI;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.OceanBills_BO) ||
						fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.OceanBills_O) ||
						fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.OceanBills_OBL))
			{
				return DocumentType.OBL;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_B) ||
						fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_BOO) ||
						fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_COM) ||
						fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_COMOBL) ||
						fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_ORD) ||
						fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_S))
			{
				return DocumentType.COM;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.HouseBills_BL) ||
				 fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.ExpressHouseBill_TLX))
			{
				if (fileName.Contains(".FREIGHTED") || fileName.Contains(".ORIGINAL"))
				{
					return DocumentType.USA;
				}
				else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.ExpressHouseBill_TLX))
				{
					return DocumentType.EBL;
				}
				else
				{
					return DocumentType.HBL;
				}
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.TransportConNote_TSP))
			{
				return DocumentType.POD;
			}
			else
			{
				return DocumentType.UKN;
			}
		}

		#endregion

		#region GetDocumentReferenceType

		ZString GetDocumentReferenceType(string fileName)
		{
			if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_ORD))
			{
				return MFIConstants.AutoeDoc.DocRefType.Order; //AssemblyDataLookup.OrderData...
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.Various_ORG))
			{
				return MFIConstants.AutoeDoc.DocRefType.Organisation;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.OceanBills_OBL) || fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_COMOBL))
			{
				return MFIConstants.AutoeDoc.DocRefType.Masterbill;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.AgentAccountNotes_AGI))
			{
				return MFIConstants.AutoeDoc.DocRefType.Container;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.AgentAccountNotes_C) || fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.OceanBills_O))
			{
				return MFIConstants.AutoeDoc.DocRefType.Consol;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_BOO))
			{
				return MFIConstants.AutoeDoc.DocRefType.Booking;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_B) || fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.OceanBills_BO))
			{
				return MFIConstants.AutoeDoc.DocRefType.Declaration;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.HouseBills_BL) || fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_COM) || fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.ExpressHouseBill_TLX))
			{
				return MFIConstants.AutoeDoc.DocRefType.Housebill;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.CommercialDocs_S))
			{
				return MFIConstants.AutoeDoc.DocRefType.Shipment;
			}
			else if (fileName.StartsWith(MFIConstants.AutoeDoc.DocumentPrefix.TransportConNote_TSP))
			{
				return MFIConstants.AutoeDoc.DocRefType.ConNotePOD;
			}
			else
			{
				return MFIConstants.AutoeDoc.DocRefType.Unknown;
			}
		}

		#endregion

		#endregion

		#endregion
	}
}
