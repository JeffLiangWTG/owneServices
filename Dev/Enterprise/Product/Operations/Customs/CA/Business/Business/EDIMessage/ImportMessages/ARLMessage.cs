using System;
using System.Data;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Messaging.Business.XmlMessaging;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.ZArchitecture.Modules;

//These are only necessary when ResourceStrings code generation is inactive (e.g. DEBUGFAST)
#pragma warning disable IDE0079
#pragma warning disable IDE0005
using Enterprise.ZArchitecture.Core;
#pragma warning restore IDE0005, IDE0079

namespace Enterprise.Customs.CA.Business
{
	public class ARLMessage : EDIMessage, IDocumentSupportable, IControllerIDProvider
	{
		public ARLMessage(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EM_ApplicationCode = XmlEDIMessage.ApplicationCodes.UniversalDataMessaging;
			EM_MessageType = EDIMessageTypeList.Codes.XDC;
			EM_MessageSubType = EDIMessageSubTypeList.Codes.XmlUniversalTransactionBatch;
		}

		#region Overrides

		public override ZDateTime K84AccountingDate
		{
			get { return this.GetSystemDefinedValue<ZDateTime>(Schema.K84AccountingDate); }
		}

		public override ZDateTime K84StatementDate
		{
			get { return this.GetSystemDefinedValue<ZDateTime>(Schema.K84StatementDate); }
		}

		public override ZString K84StatementBN9
		{
			get
			{
				var transactionBatch = GetEM_MessageTextReader().Parse<TransactionBatch>();
				return transactionBatch?.OrganizationAddressCollection?.FirstOrDefault(orgAddress => (orgAddress?.AddressType ?? ZString.Empty) == "ImportBroker")?.GovRegNum ?? ZString.Empty;
			}
		}

		public override ZString XMLCustomsMessageType
		{
			get { return this.GetSystemDefinedValue<ZString>(Schema.XMLCustomsMessageType); }
		}

		[BusinessObjectTestExclude]
		public override ZString EM_MessageSubType
		{
			get
			{
				var customsMessageType = XMLCustomsMessageType;
				return customsMessageType.IsEmpty ? base.EM_MessageSubType : customsMessageType;
			}
			set
			{
				base.EM_MessageSubType = value;
			}
		}

		protected override CodeDescriptionPairList MessageSubTypeList
		{
			get { return new ARLMessageTypes(); }
		}

		#region Properties

		protected override bool ShouldUseUnformattedMessageText
		{
			get { return false; }
		}

		public override bool ShouldShowInterpretation
		{
			get { return true; }
		}

		#endregion

		#endregion

		#region IDocumentSupportable Members

		DocumentSupporter IDocumentSupportable.DocumentSupporter
		{
			get { return DocumentSupporter; }
		}

		ARLDocumentSupporter DocumentSupporter
		{
			get { return new ARLDocumentSupporter(this); }
		}

		#endregion

		#region IControllerIDProvider Members

		Guid IControllerIDProvider.BusinessObjectPK
		{
			get { return PK.ToGuid(); }
		}

		ControllerID IControllerIDProvider.ControllerID
		{
			get { return ControllerIDs.Customs.CA.K84Reports; }
		}

		#endregion
	}
}
