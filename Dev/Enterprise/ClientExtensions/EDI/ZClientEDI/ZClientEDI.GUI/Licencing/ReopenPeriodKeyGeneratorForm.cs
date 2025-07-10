using System;
using Enterprise.Client.EDI.LicenceKeyBuilder.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	public partial class ReopenPeriodKeyGeneratorForm : ZChildForm
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public ReopenPeriodKeyGeneratorForm()
		{
			InitializeComponent();
		}

		public ReopenPeriodKeyGeneratorForm(ReopenPeriodKeyBusinessObject bo)
			: base(bo)
		{
			InitializeComponent();
		}

		public override string FormVerb
		{
			get { return string.Empty; }
		}

		void GenerateButton_Click(object sender, EventArgs e)
		{
			ReopenPeriodKeyBusinessObject bo = (ReopenPeriodKeyBusinessObject)BusinessEntity;
			try
			{
				bo.GenerateKey();
			}
			catch (InvalidOperationException ex)
			{
				Globals.Message.Show(ex.Message);
			}
		}
	}
}

