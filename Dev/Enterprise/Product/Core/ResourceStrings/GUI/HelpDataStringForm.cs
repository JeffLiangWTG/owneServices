using System;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Tools.SpellCheck;
using CargoWise.Tools.SpellCheck.GUI;
using CargoWise.Windows.UI;
using Enterprise.ResourceStrings.Business;
using Enterprise.ZArchitecture.GUI;
using WTG.SpellCheck;

namespace Enterprise.ResourceStrings.GUI
{
	public partial class HelpDataStringForm : ZForm, IButtonDeleteTextOverride
	{
		public HelpDataStringForm()
		{
			InitializeComponent();
		}

		public HelpDataStringForm(HelpDataString bizO) : base(bizO)
		{
			InitializeComponent();

			bizO.HD_FullDescriptionInfo.ValueChanged += DescriptionChanged;
			buttonFixRN.Visible = HasWrongRN();
			SetupDocBuilderUsageButton();
		}

		protected override void OnLoad(EventArgs e)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, postingButtonsUserControl);
			base.OnLoad(e);
		}

		protected override void Save(CargoWise.Integration.ITransactionParticipant[] factories)
		{
			ResourceStringsFactory.Save((HelpDataString)this.BusinessEntity);
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}

			HelpDataString helpDataString = BusinessEntity as HelpDataString;
			if (helpDataString != null)
			{
				helpDataString.HD_FullDescriptionInfo.ValueChanged -= DescriptionChanged;
			}

			base.Dispose(disposing);
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		public string DeleteButtonText
		{
			get { return Res.GetString("05550739-baa9-4653-88e8-1c439d754e93", "Undo"); }
		}

		protected override void Delete()
		{
			ResourceStringsFactory.UndoCheckout((HelpDataString)BusinessEntity);
		}

		protected override ContinueWithSave ValidateAndSave()
		{
			if (RunSpellCheck() == ControlSpellCheckerResult.Cancel)
			{
				return ContinueWithSave.No;
			}
			else
			{
				return base.ValidateAndSave();
			}
		}

		ControlSpellCheckerResult RunSpellCheck()
		{
			if (((HelpDataString)BusinessEntity).HD_Language == Res.DefaultLanguage)
			{
				ControlSpellChecker spellChecker = new ControlSpellChecker(GetCaptionsSpellchecker(),
					ShortCaptionTextBox,
					MediumCaptionTextBox,
					CaptionTextBox);
				spellChecker.AddControl(GetDescriptionSpellchecker(),
					DescriptionTextBox);
				return spellChecker.CheckSpelling();
			}
			else
			{
				return ControlSpellCheckerResult.NoErrors;
			}
		}

		internal static ISpellChecker GetCaptionsSpellchecker()
		{
			return EnterpriseResourceStringSpellChecker.GetCaptionsSpellchecker();
		}

		internal static ISpellChecker GetDescriptionSpellchecker()
		{
			return EnterpriseResourceStringSpellChecker.GetFullDescriptionSpellchecker();
		}

		void buttonFixRN_Click(object sender, EventArgs e)
		{
			var entity = (HelpDataString)BusinessEntity;
			entity.HD_FullDescription = FixRN(entity.HD_FullDescription);
		}

		protected override bool AllowNew
		{
			get
			{
				return false;
			}
		}

		internal bool TestAllowNew
		{
			get
			{
				return AllowNew;
			}
		}

		internal static string FixRN(string text)
		{
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < text.Length; i++)
			{
				if (text[i] == '\n' && (i == 0 || text[i - 1] != '\r'))
				{
					if (i < text.Length - 1 && text[i + 1] == '\r')
					{
						i++;
					}
					else
					{
						sb.Append('\r');
					}
				}
				sb.Append(text[i]);
				if (text[i] == '\r' && (i == text.Length - 1 || text[i + 1] != '\n'))
				{
					sb.Append('\n');
				}
			}
			return sb.ToString();
		}

		bool HasWrongRN()
		{
			string text = ((HelpDataString)BusinessEntity).HD_FullDescription;
			for (int i = 0; i < text.Length; i++)
			{
				if ((text[i] == '\n' && (i == 0 || text[i - 1] != '\r')) ||
					(text[i] == '\r' && (i == text.Length - 1 || text[i + 1] != '\n')))
				{
					return true;
				}
			}
			return false;
		}

		void DescriptionChanged(object sender, EventArgs e)
		{
			buttonFixRN.Visible = HasWrongRN();
		}

		void docBuilderUsageButton_Click(object sender, EventArgs e)
		{
			if (!DocBuilderUsageFinder.IsInitialized)
			{
				using (var progressForm = new ProgressForm())
				{
					progressForm.Status = "Analyzing DocStrips";
					progressForm.ShowCancelButton = false;
					progressForm.ShowProgressBar = false;
					progressForm.Show();
					progressForm.Invalidate();
					progressForm.Update();
					DocBuilderUsageFinder.Initialize();
					SetupDocBuilderUsageButton();
				}
			}
			if (docBuilderUsageButton.Enabled)
			{
				new DocBuilderUsagesForm((HelpDataString)this.BusinessEntity).Show();
			}
		}

		void SetupDocBuilderUsageButton()
		{
			if (DocBuilderUsageFinder.IsInitialized)
			{
				var docBuilderUsages = ((HelpDataString)this.BusinessEntity).DocBuilderUsages;
				if (docBuilderUsages == null || docBuilderUsages.Count == 0)
				{
					docBuilderUsageButton.CaptionResourceString = Res.GetData("184686FB-1852-4615-B70B-B614756BBA1F", "No DocBuilder Usage");
					docBuilderUsageButton.Enabled = false;
				}
				else
				{
					docBuilderUsageButton.CaptionResourceString = Res.GetData("CB776718-E581-4D0C-8519-DBAF258BE201", "View DocBuilder Usage");
				}
				docBuilderUsageButton.Extensions.Get<ILabelCaptionRenderer>().Refresh();
			}
		}

		IDocBuilderUsageFinder DocBuilderUsageFinder
		{
			get { return docBuilderUsageFinder ?? (docBuilderUsageFinder = ObjectFactory.Get<IDocBuilderUsageFinder>()); }
		}
		IDocBuilderUsageFinder docBuilderUsageFinder;
	}
}
