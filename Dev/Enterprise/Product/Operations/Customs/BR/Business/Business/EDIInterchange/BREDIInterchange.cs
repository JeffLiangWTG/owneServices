using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.BR.Registry;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.BR.Business
{
	public class BREDIInterchange : EDIInterchange, Integration.Customs.BR.IEDIInterchange, IMessageDataProvider
	{
		public const string BRCustoms = "BRCustoms";
		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:Res.GetString Analyzer", Justification = "Place holder text")]
		public const string EndpointAddressPlaceHolder = "<<ENDPOINT PLACE HOLDER>>";

		public BREDIInterchange(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		protected override void SetDefaultValues()
		{
			base.SetDefaultValues();
			EI_ApplicationCode = ApplicationCodes.BRCustoms;
		}

		protected override bool ShouldSendViaEHubCore => EI_TransportType != EDIInterchange.TransportType.xT;

		protected override Type GetMessageTypeToCreate(ZString messageText) => typeof(BREDIMessage);

		public override bool IsTestInterchange => false;

		protected override bool ShouldBatchNumberBeByInterchange => true;

		protected override string UniqueBatchNumberPlaceHolderOverride => EDIMessage.UniqueBatchNumberPlaceHolderHtml;

		protected override string GetBatchNumber()
		{
			if (EI_InterchangeType == MessageTypeList.Codes.LIC)
			{
				var batchNumber = Env.NumberFountains.GetBRLicenseMessageBatchNumber().GetNextFormatted(Factory);
				ContainedMessages.Cast<EDIMessage>().ForEach(m => m.EM_ApplicationReference = batchNumber);
				return batchNumber;
			}
			else
			{
				return base.GetBatchNumber();
			}
		}

		BinaryReader IMessageDataProvider.GetMessageData()
		{
			return EI_InterchangeType == MessageTypeList.Codes.SUB ? GetEI_BodyTextReaderReplaceEndpointAddress() : new BinaryReader(GetEI_BodyTextReader().CopyAndDispose());
		}

		BinaryReader GetEI_BodyTextReaderReplaceEndpointAddress()
		{
			var company = GlbCompany.CurrentCompany;
			var messageData = EI_BodyText.Replace(EndpointAddressPlaceHolder, BRCustomsDataRegistry.Instance.XTEndPointAddress.Value + "/" + company.LicenceEnterpriseCode + company.LicenceServerID);
			return new BinaryReader(new MemoryStream(Encoding.UTF8.GetBytes(messageData.ToString())));
		}
	}
}
