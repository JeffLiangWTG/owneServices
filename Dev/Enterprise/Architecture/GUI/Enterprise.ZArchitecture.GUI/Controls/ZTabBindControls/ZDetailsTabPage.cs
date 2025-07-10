using System;
using System.Windows.Forms;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.GUI
{
	/// <summary>
	/// A ZTabPage with the ability to show Master Header Text in Tab Page Text to make it easier for user to understand the Master Child tab relationship
	/// </summary>
	public class ZDetailsTabPage : ZTabPage
	{
		public ZDetailsTabPage()
			: base()
		{
		}

		public void Hook(CurrencyManager parentListManager)
		{
			fParentListManager = parentListManager;
			if (fParentListManager != null)
			{
				fParentListManager.CurrentChanged += new EventHandler(ListManager_CurrentChanged);
				ListManager_CurrentChanged(this, EventArgs.Empty);
			}
		}

		public void Unhook()
		{
			if (fParentListManager != null)
			{
				fParentListManager.CurrentChanged -= new EventHandler(ListManager_CurrentChanged);
			}
		}

		public string AdditionalText
		{
			get { return fAdditionalText; }
			set { fAdditionalText = value; }
		}
		string fAdditionalText = Res.GetString("0c2ffa97-01d5-4574-ac0b-95cd5d72db31", "Details");

		CurrencyManager fParentListManager;

		void ListManager_CurrentChanged(object sender, EventArgs e)
		{
			string newText;

			if (fParentListManager.Position >= 0 &&
				fParentListManager.GetCurrent() != null &&
				fParentListManager.GetCurrent() is IDetailsTabPageHeadingProvider)
			{
				newText = ((IDetailsTabPageHeadingProvider)fParentListManager.GetCurrent()).Heading + " " + AdditionalText;
			}
			else
			{
				newText = AdditionalText;
			}

			if (Text != newText)
			{
				Text = newText;
			}
		}
	}
}
