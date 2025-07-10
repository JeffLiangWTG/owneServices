using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.Riba;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.Riba
{
	public partial class BackDatePostForm : ZChildForm
	{
		public BackDatePostForm()
		{ }

		public BackDatePostForm(BackDatePostHolder backDatePostHolder)
			: base(backDatePostHolder)
		{
			InitializeComponent();
		}

		BackDatePostHolder BackDatePostHolder
		{
			get
			{
				return BusinessEntity as BackDatePostHolder;
			}
		}

		void OKButton_Click(object sender, EventArgs e)
		{
			if (BackDatePostHolder.HasErrors)
			{
				Globals.Message.ShowError(Res.GetString("b135d9ab-69fd-4806-a070-51358ffded6c", "Please choose a valid date"));
			}
			else
			{
				DialogResult = DialogResult.OK;
				Close();
			}
		}

		void CancelButton_Click(object sender, EventArgs e)
		{
			((BackDatePostHolder)BusinessEntity).BackPostDate = ZDate.Today;
			((BackDatePostHolder)BusinessEntity).BackInvoiceDate = ZDate.Today;
			Close();
		}
	}
}
