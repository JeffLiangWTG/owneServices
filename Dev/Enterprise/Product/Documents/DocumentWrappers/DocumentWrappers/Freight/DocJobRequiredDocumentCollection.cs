using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Res = DocumentWrappers.Res;

namespace Enterprise.DocumentWrappers
{
	public class DocJobRequiredDocumentCollection : DocumentWrapperCollection
	{
		public DocJobRequiredDocumentCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DocJobRequiredDocumentCollection(JobRequiredDocumentDependentCollection collectionSource, BusinessObjectFactory factoryToWrap)
			: base(collectionSource, factoryToWrap)
		{
		}

		public new DocJobRequiredDocument this[int index]
		{
			get { return (DocJobRequiredDocument)base[index]; }
		}

		public DocJobRequiredDocument GetDocByType(ZString docType)
		{
			foreach (DocJobRequiredDocument doc in this)
			{
				if (doc.Type == docType)
				{
					return doc;
				}
			}
			return null;
		}

		public DocJobRequiredDocument OriginalBill
		{
			get { return GetDocByType(Core.Constants.RefDocTypes.MasterBill); }
		}

		public DocJobRequiredDocument OriginalOceanBill
		{
			get { return GetDocByType(Core.Constants.RefDocTypes.OceanMasterBill); }
		}

		public DocJobRequiredDocument PackingDeclaration
		{
			get { return GetDocByType(Core.Constants.RefDocTypes.QuarantinePackingDeclaration); }
		}

		public DocJobRequiredDocument CertificateOfOrigin
		{
			get { return GetDocByType(Core.Constants.RefDocTypes.CertificateOfOrigin); }
		}

		public DocJobRequiredDocument CommercialInvoice
		{
			get { return GetDocByType(Core.Constants.RefDocTypes.CommercialInvoice); }
		}

		public DocJobRequiredDocument FumigationCertificate
		{
			get { return GetDocByType(Core.Constants.RefDocTypes.FumigationCertificate); }
		}

		public DocJobRequiredDocument PackingList
		{
			get { return GetDocByType(Core.Constants.RefDocTypes.PackingList); }
		}

		public DocJobRequiredDocument ManufacturersDec
		{
			get { return GetDocByType(Core.Constants.RefDocTypes.ManufacturersDeclaration); }
		}

		public DocJobRequiredDocument FoodControlCert
		{
			get { return GetDocByType(Core.Constants.RefDocTypes.FoodControlCertificate); }
		}

		public DocJobRequiredDocument VeterinaryCert
		{
			get { return GetDocByType(Core.Constants.RefDocTypes.VetinaryCertificate); }
		}

		public DocJobRequiredDocument HealthCert
		{
			get { return GetDocByType(Core.Constants.RefDocTypes.HealthCertificate); }
		}

		public DocJobRequiredDocument MotorVehicleCert
		{
			get { return GetDocByType(Core.Constants.RefDocTypes.MotorVehicleCertificate); }
		}

		public ZString MissingRequiredDocuments
		{
			get
			{
				ZString result = ZString.Empty;
				SortInfo sortInfo = new SortInfo((NoResString)"Type", ListSortDirection.Ascending);
				Sort(sortInfo);
				foreach (DocJobRequiredDocument doc in this)
				{
					result += FormatMissingDocument(doc);
				}
				return result.TrimEnd();
			}
		}

		#region Implementation

		protected ZString FormatMissingDocument(DocJobRequiredDocument doc)
		{
			ZString result = "";

			if (doc != null && !doc.IsReceived && !doc.Description.IsEmpty)
			{
				result = "- " + doc.Description;
				result += (!doc.IsOriginalRequired) ? "\n" : " " + Res.GetString("e598d045-b638-4f5f-be06-70cf0b1e4e44", "- Original Required") + "\n";
			}
			return result;
		}

		#endregion
	}
}
