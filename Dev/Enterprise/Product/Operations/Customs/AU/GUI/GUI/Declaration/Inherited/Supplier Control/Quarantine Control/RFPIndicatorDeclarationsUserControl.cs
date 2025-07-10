using System;
using CargoWise.Types;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class RFPIndicatorDeclarationsUserControl : ZUserControl
	{
		public RFPIndicatorDeclarationsUserControl()
		{
			InitializeComponent();
		}

		JobComInvoiceHeader CurrentInvoice
		{
			get
			{
				if (currentInvoice?.IsDeleted ?? false)
				{
					currentInvoice = null;
				}
				return currentInvoice;
			}
			set
			{
				currentInvoice = value;
			}
		}
		JobComInvoiceHeader currentInvoice;

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);

			var invoice = CurrentDataItem as JobComInvoiceHeader;

			if (CurrentInvoice != invoice)
			{
				UnHookEvents();

				CurrentInvoice = invoice;

				HookEvents();
			}
		}

		void UnHookEvents()
		{
			var exDocHeader = CurrentInvoice?.QuarantineExDocHeader;

			if (exDocHeader != null)
			{
				exDocHeader.QH_ProduceTypeInfo.ValueChanged -= QH_ProduceTypeInfo_ValueChanged;
			}
		}

		void HookEvents()
		{
			var exDocHeader = CurrentInvoice?.QuarantineExDocHeader;

			if (exDocHeader != null)
			{
				exDocHeader.QH_ProduceTypeInfo.ValueChanged += QH_ProduceTypeInfo_ValueChanged;
			}

			ProduceTypeValueChanged();
		}

		void QH_ProduceTypeInfo_ValueChanged(object sender, EventArgs e)
		{
			ProduceTypeValueChanged();
		}

		void ProduceTypeValueChanged()
		{
			var header = CurrentInvoice?.QuarantineExDocHeader;
			var produceType = header?.QH_ProduceType ?? ZString.Empty;

			switch (produceType)
			{
				case EXDOCCommodityCodes.Codes.Meat:
					{
						DeclarationOfComplianceLabel.Text = ExDocMeatComplianceMessage;
						break;
					}
				case EXDOCCommodityCodes.Codes.GrainsAndPlants:
				case EXDOCCommodityCodes.Codes.Horticulture:
					{
						DeclarationOfComplianceLabel.Text = ExDocGrainsAndHorticultureComplianceMessage;
						break;
					}
				case EXDOCCommodityCodes.Codes.Dairy:
				case EXDOCCommodityCodes.Codes.Eggs:
				case EXDOCCommodityCodes.Codes.Fish:
					{
						DeclarationOfComplianceLabel.Text = ExDocDairyComplianceMessage;
						break;
					}
				default:
					{
						DeclarationOfComplianceLabel.Text = DefaultComplianceMessage;
						break;
					}
			}

			ImportedProductsLabel.Text = produceType == EXDOCCommodityCodes.Codes.Fish
				? Res.GetString("0EC8681F-46CC-4374-AD89-9AB139B5A41E", "Are any of the products listed in the RFP imported?")
				: Res.GetString("5663A438-FC20-41DB-8533-2870B5129CEA", "Do any of the products listed in this RFP contain imported dairy ingredients, other than from New Zealand?");

			TrueAndCompleteLabel.Text = produceType == EXDOCCommodityCodes.Codes.Meat
				? "Do you have effective measures in place to ensure that the information contained in this request for permit is accurate and complete? Note: For criminal penalties applying to persons who make false or misleading statements to a Commonwealth entity see the Criminal Code Act 1995 Part 7.4 (false or misleading statements)."
				: "Is all the information given in this application for an export permit true and complete?";
		}

		string ExDocMeatComplianceMessage => @"Do you declare that you have evidence supporting:
i. the importing country requirements applicable to the goods have been met or will be met before the goods are imported into the importing country
ii. the goods have been prepared in compliance with the Export Control Act 2020
iii. I have supervised, or have been given a declaration by the person who supervised, the loading of the goods to which this application relates stating that the prescribed export conditions, and any other conditions that apply in relation to the goods under the Act, have been complied with; and the importing country requirements relating to the goods are met
iv. the information supplied on this form is true and correct.";  // Specific text required by Australian Customs

		string ExDocDairyComplianceMessage
		{
			get { return "Is the exporter in possession of either: (a) A declaration that complies with Part 4 of Chapter 7 of the relevant Rules: or (b) A written notice provided by an assessor made under subsection 9 - 20(2) of Chapter 9 of the relevant Rules(Y/ N)"; }  // Specific text required by Australian Customs
		}

		string ExDocGrainsAndHorticultureComplianceMessage => @"Do you declare that:
i. the importing country requirements applicable to the goods have been verified with importing country’s National Plant Protection Organisation, and any changes have been provided to the department;
ii. the goods have been prepared in accordance with relevant conditions or restrictions prescribed under the Export Control Act 2020 and, where required, are ready for assessment;
iii. the information supplied on this form is true and correct.
Note: A person may commit an offence or be liable to a civil penalty under the Export Control Act 2020 or Criminal Code Act 1995 if the person makes a false or misleading statement in an application or provides false or misleading information or documents."; // Specific text required by Australian Customs

		string DefaultComplianceMessage
		{
			get { return "Is the exporter in possession of either: (a) a declaration that complies with clause 6 of Schedule 9 of the relevant Orders; or (b) a written verification by an authorised officer made under clause 8 of Schedule 9 of the relevant Orders?"; }   // Specific text required by Australian Customs
		}

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			UnHookEvents();

			if (disposing && (components != null))
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}
	}
}
