using System;
using Enterprise.Customs.Business;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.EU.GUI;

namespace Enterprise.Customs.ES.GUI
{
	public partial class JobDeclarationUserControl : EUJobDeclarationUserControl
	{
		public JobDeclarationUserControl()
		{
			InitializeComponent();
		}

		JobDeclaration ESDeclaration => (JobDeclaration)JobDeclaration;

		public override BaseJobDeclaration JobDeclaration
		{
			get => base.JobDeclaration;
			set
			{
				UnhookJobDeclarationEvents(ESDeclaration);
				base.JobDeclaration = value;
				HookJobDeclarationEvents(ESDeclaration);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				UnhookJobDeclarationEvents(ESDeclaration);
			}
			base.Dispose(disposing);
		}

		void HookJobDeclarationEvents(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				declaration.JE_HouseBillInfo.ValueChanged += JE_HouseBillInfo_ValueChanged;
			}
		}

		void UnhookJobDeclarationEvents(JobDeclaration declaration)
		{
			if (declaration != null)
			{
				declaration.JE_HouseBillInfo.ValueChanged -= JE_HouseBillInfo_ValueChanged;
			}
		}

		void JE_HouseBillInfo_ValueChanged(object sender, EventArgs e)
		{
			if (ESDeclaration.Entries.Count > 0)
			{
				TransportDocumentHelper.AddOrCopyTransportDocumentToMisc(ESDeclaration);
			}
		}

		protected override Type GetCustomsOfficesUserControlType() => typeof(CustomsOfficesUserControl);
	}
}
