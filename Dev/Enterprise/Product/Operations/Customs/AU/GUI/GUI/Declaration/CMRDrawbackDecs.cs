using System;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	/// <summary>
	/// Summary description for CMRDrawbackDecsForm.
	/// </summary>
	public partial class CMRDrawbackDecsForm : ZChildForm
	{
		public CMRDrawbackDecsForm(JobDeclaration declaration)
			: base(declaration)
		{
			this.declaration = declaration;
		}

		readonly JobDeclaration declaration;

		public override string FormCaption
		{
			get { return "Lodgement Questions"; }
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ValidateDeclaration();
		}

		protected virtual void ValidateDeclaration()
		{
			declaration.RunPreSaveValidation();
		}

		public bool IsOKToProceed;
		void OKButton_Click(object sender, EventArgs e)
		{
			IsOKToProceed = true;
			Close();
		}

		void CPQACancelButton_Click(object sender, EventArgs e)
		{
			IsOKToProceed = false;
			Close();
		}
	}
}
