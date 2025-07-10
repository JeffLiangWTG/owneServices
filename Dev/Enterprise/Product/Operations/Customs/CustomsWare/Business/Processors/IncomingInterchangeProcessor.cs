using CargoWise.Common;
using CargoWise.Types;
using Enterprise.BatchProcessor;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.CustomsWare.Business
{
	public class IncomingInterchangeProcessor : InboundInterchangeProcessor
	{
		public IncomingInterchangeProcessor()
		{
		}

		protected IncomingInterchangeProcessor(LoggingInformation logger)
			: base(logger)
		{
		}

		protected override string[] ApplicationCodes
		{
			get { return new string[] { ApplicationCodeList.Codes.CustomsWare }; }
		}

		protected override IInboundMessageCreator GetMessageCreator(EDIInterchange interchange)
		{
			return new MessageCreator();
		}

		protected override ZString GetReasonForCannotProcessInterchangeCore()
		{
			var customsWareCompanyCode = CustomsDataRegistry.Instance.CustomsWareCompany.Value;
			return customsWareCompanyCode.IsNullOrEmpty() ? (ZString)Res.GetString("74917F79-907E-4ED2-A0E6-E1313833BBB0", "Please set up the ABM Web Service Company (code '{0}') in the registry ({1}).", GlbCompany.CurrentCompany.GC_Code, CustomsDataRegistry.Instance.CustomsWareCompany.HumanReadableRegistryPath()) : ZString.Empty;
		}
	}
}

