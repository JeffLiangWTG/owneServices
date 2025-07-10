using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.CA.Messaging;
using Enterprise.Customs.Common.MessageBuilders;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.Customs.CA.Business.MessageBuilders
{
	public class EDIReleaseImportMessageWrapper : IEDIReleaseOGD
	{
		public EDIReleaseImportMessageWrapper(CusEntryHeader entryHeader)
		{
			CanSendDeclarationChecker.EntryNotNullAndAttachedToDeclaration(entryHeader);
			this.entryHeader = entryHeader;
			declaration = entryHeader.Declaration;
			weightUQCalculator = new WeightUQCalculator(entryHeader);
			netWeightUQCalculator = new NetWeightUQCalculator(entryHeader);
		}

		public void PopulateEntrySubmittedDateIfRequired()
		{
			this.entryHeader.PopulateEntrySubmittedDateIfRequired();
		}

		#region IEDIRelease Members

		#region ICAEDIFACTMessageAttachee Members

		public BusinessObjectFactory Factory
		{
			get { return declaration.Factory; }
		}

		public BusinessObject TopLevelBusinessObject
		{
			get { return declaration; }
		}

		public void AddMessage(Enterprise.Messaging.Business.EDIMessage message)
		{
			entryHeader.Messages.Add(message);
		}

		public Enterprise.Messaging.Business.EDIMessageCollection Messages
		{
			get { return entryHeader.Messages; }
		}

		public ZString MessageStatus
		{
			get { return entryHeader.CH_Status; }
			set { entryHeader.CH_Status = value; }
		}

		public ZString JobStatus
		{
			get { return entryHeader.CH_EntryStatus; }
			set { entryHeader.CH_EntryStatus = value; }
		}

		public bool HasChanges
		{
			get { return declaration.HasChanges; }
		}

		public ZString JobIdentification
		{
			get { return declaration.JE_DeclarationReference; }
		}

		bool ICAEDIFACTMessageAttachee.IsCancelled
		{
			get { return declaration.IsCancelled; }
		}

		bool IEDIFACTMessageAttachee.RefreshValidationBeforeSendMessage
		{
			get { return declaration.RefreshValidationBeforeSendMessage; }
		}

		#endregion

		#region IEDIReleaseMin Members

		public ZString TransactionNumber
		{
			get { return declaration.TransactionNumber; }
		}

		public ZString ServiceOptionID
		{
			get { return declaration.CA_ServiceOption; }
		}

		public ZString AssessmentOption
		{
			get { return declaration.CA_AssesmentOption; }
		}

		public ZString ImporterNumber
		{
			get
			{
				var result = ZString.Empty;
				if (declaration.ImporterOfRecordAddress.HasRealOrganisation)
				{
					result = GetImporterNumber(declaration.ImporterOfRecordAddress.Organisation);
				}
				else if (!declaration.JE_OH_Importer.IsEmpty)
				{
					result = GetImporterNumber(declaration.Importer);
				}
				return result;
			}
		}

		ZString GetImporterNumber(OrgHeader orgHeader)
		{
			var result = ZString.Empty;
			var orgCusCode = orgHeader.CustomsCodes.GetOrgCusCodeObjectForCodeAndCountry(OrgCusCode.CACodeTypes.BusinessNumberForImportExport, Core.Constants.CountryCodes.Canada);
			if (orgCusCode != null)
			{
				result = orgCusCode.OK_CustomsRegNo;
			}

			return result;
		}

		public ZString PriorityIndicator
		{
			get { return declaration.CA_PriorityInd; }
		}

		public ZString PortOfClearance
		{
			get { return declaration.JE_CustomsOffice; }
		}

		public ZString GoodsLocationCode
		{
			get { return declaration.JE_LocationOfGoods; }
		}

		public ZString GoodsLocationName
		{
			get { return GoodsLocationCode.IsEmpty ? declaration.CA_SubLocationName : ZString.Empty; }
		}

		public ZDateTime DateOfArrival
		{
			get { return IsPreArrival ? declaration.JE_DateOfFirstArrival : ZDateTime.Empty; }
		}

		public ZDateTime DateOfDeparture
		{
			get { return IsPostArrival ? declaration.JE_WarehouseReleaseDate : ZDateTime.Empty; }
		}

		public ZDecimal GrossWeight
		{
			get { return weightUQCalculator.Weight; }
		}

		public ZString GrossWeightUnits
		{
			get { return weightUQCalculator.UQ; }
		}

		public ZDecimal NetWeight
		{
			get { return netWeightUQCalculator.Weight; }
		}

		public ZString NetWeightUnits
		{
			get { return netWeightUQCalculator.UQ; }
		}

		public ZString[] ContainerNumbers
		{
			get { return declaration.CusContainers.ContainerNumbers.ToArray(); }
		}

		public ZString[] CargoControlNumbers
		{
			get
			{
				return (from CargoControlNumber cnn in declaration.CargoControlNumbers where !cnn.CY_CargoControlNumber.IsEmpty select cnn.CY_CargoControlNumber).ToArray();
			}
		}

		public ZInt[] NumberOfPackages
		{
			get { return (from BasePackage package in declaration.Packages select package.CW_PackQty).ToArray(); }
		}

		public ZString[] TypeOfPackages
		{
			get { return (from BasePackage package in declaration.Packages select package.CW_PackType).ToArray(); }
		}

		public IDocAddress Importer
		{
			get { return declaration.ImporterOfRecordAddress.HasRealOrganisation ? declaration.ImporterOfRecordAddress : declaration.ImporterDocumentaryAddress; }
		}

		public IDocAddress Carrier
		{
			get { return new DocAddressWrapper(declaration.CA_CarrierName); }
		}

		public IDocAddress Broker
		{
			get { return declaration.CusAgent == null ? null : new DocAddressWrapper(declaration.CusAgent.GS_FullName); }
		}

		public IDocAddress DeliveryAddress
		{
			get { return declaration.ClientPickupDeliveryAddress; }
		}

		public ZString DeliveryInstructions
		{
			get { return entryHeader.CH_CustomsDeliveryInstructions; }
		}

		public ZDecimal TotalValueForDuty
		{
			get { return entryHeader.CustomsValue; }
		}

		public IEnumerable<IEDIInvoiceOGD> Invoices
		{
			get { return entryHeader.InvoiceHeaders; }
		}

		#endregion

		#region IEDIReleaseOGD Members

		public ZBool OGDCFIA
		{
			get { return declaration.CA_OGDCFIA; }
		}

		public ZBool OGDIC
		{
			get { return declaration.CA_OGDIC; }
		}

		public ZBool OGDNR
		{
			get { return declaration.CA_OGDNR; }
		}

		public ZBool OGDTC
		{
			get { return declaration.CA_OGDTC; }
		}

		public ZString DeliveryPhone
		{
			get { return declaration.ClientPickupDeliveryAddress.E2_Phone; }
		}

		public ZString DeliveryFax
		{
			get { return declaration.ClientPickupDeliveryAddress.E2_Fax; }
		}

		#endregion

		#endregion

		#region Implementation

		bool IsPostArrival
		{
			get { return ServiceOptionID == ServiceOptions.Codes.RMDOGD; }
		}

		bool IsPreArrival
		{
			get { return ServiceOptionID == ServiceOptions.Codes.PARS || ServiceOptionID == ServiceOptions.Codes.PARSOGD; }
		}

		readonly CusEntryHeader entryHeader;
		readonly JobDeclaration declaration;
		readonly WeightUQCalculator weightUQCalculator;
		readonly NetWeightUQCalculator netWeightUQCalculator;

		#endregion
	}
}
