using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.Types;
using Enterprise.Customs.GB.H7.Business;

namespace Enterprise.Customs.GB.H7.Messaging
{
	public abstract class H7ArrivalAndCancelBaseMessageBuilder
	{
		protected readonly MessageSendingObject ObjectToSend;
		protected readonly string functionCode;

		public H7ArrivalAndCancelBaseMessageBuilder(MessageSendingObject objectToSend, string functionCode)
		{
			this.ObjectToSend = objectToSend;
			this.functionCode = functionCode;
		}

		public abstract ZString Build();

		protected MetaData CreateMetaData()
		{
			return new MetaData
			{
				WCODataModelVersionCode = new MetaDataWCODataModelVersionCodeType
				{
					Value = "3.6"
				},
				WCOTypeName = new MetaDataWCOTypeNameTextType
				{
					Value = "DEC"
				},
				ResponsibleCountryCode = new MetaDataResponsibleCountryCodeType
				{
					Value = "GB"
				},
				ResponsibleAgencyName = new MetaDataResponsibleAgencyNameTextType
				{
					Value = "HMRC"
				},
				AgencyAssignedCustomizationVersionCode = new MetaDataAgencyAssignedCustomizationVersionCodeType
				{
					Value = "v2.1"
				}
			};
		}

		protected Declaration CreateMetaDeclaration()
		{
			return new Declaration
			{
				FunctionCode = new DeclarationFunctionCodeType
				{
					Value = functionCode
				},
				FunctionalReferenceID = new DeclarationFunctionalReferenceIDType
				{
					Value = ObjectToSend.Bill.LocalReferenceNumber
				},
				ID = new DeclarationIdentificationIDType
				{
					Value = ObjectToSend.Bill.MovementReferenceNumber
				},
				TypeCode = new DeclarationTypeCodeType
				{
					Value = TypeCode
				},
				AdditionalInformation = AdditionalInformation,
				Amendment = Amendments?.ToArray()
			};
		}

		protected abstract ZString TypeCode { get; }

		protected abstract DeclarationAdditionalInformation[] AdditionalInformation { get; }

		protected abstract IEnumerable<DeclarationAmendment> Amendments { get; } 
	}
}
