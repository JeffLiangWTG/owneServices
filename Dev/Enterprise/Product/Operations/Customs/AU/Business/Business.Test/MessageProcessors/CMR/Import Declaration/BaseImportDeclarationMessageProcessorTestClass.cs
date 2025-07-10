using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	class BaseImportDeclarationMessageProcessorTestClass : BaseImportDeclarationMessageProcessor
	{
		public BaseImportDeclarationMessageProcessorTestClass()
			: base(null, "", "")
		{
		}

		public ZGuid AcknowledgementEmailGroupExposed
		{
			get { return base.AcknowledgementEmailGroup; }
		}

		public ZString AcknowledgementEmailModeExposed
		{
			get { return base.AcknowledgementEmailMode; }
		}

		public ZGuid ImpedimentEmailGroupExposed
		{
			get { return base.ImpedimentEmailGroup; }
		}

		public ZString ImpedimentEmailModeExposed
		{
			get { return base.ImpedimentEmailMode; }
		}

		public ZGuid ErrorEmailGroupExposed
		{
			get { return base.ErrorEmailGroup; }
		}

		public ZString ErrorEmailModeExposed
		{
			get { return base.ErrorEmailMode; }
		}
	}
}
