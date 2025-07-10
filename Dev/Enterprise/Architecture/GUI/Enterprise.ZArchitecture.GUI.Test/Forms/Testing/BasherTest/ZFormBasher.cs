using System;
using System.Windows.Forms;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Testing
{
	[TestedType(typeof(ZForm))]
	[DoNotAddToTestTree]
	[TestExcludeZWinFormsAllHaveFormBashers]
	public sealed class ZFormBasher : ZFormBasherTest
	{
		public ZFormBasher(Form formToBash)
		{
			fForm = formToBash;
		}

		public void BashFormWithoutMemoryChecking()
		{
			BashForm();
		}

		public new Type FormToBashType
		{
			get { return fForm.GetType(); }
		}

		protected override Form GetFormToBashCore()
		{
			return fForm;
		}

		readonly Form fForm;
	}
}
