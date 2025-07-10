using System;
using Enterprise.Customs.KR.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	public partial class ImportValuationDeclarationUserControl : ZUserControl
	{
		[Obsolete("Use the constructor that takes a business object, this constructor is just for the designer", true)]
		public ImportValuationDeclarationUserControl()
		{
			InitializeComponent();
		}

		public ImportValuationDeclarationUserControl(ZGrid messageSendingObjectsGrid)
		{
			this.messageSendingObjectsGrid = messageSendingObjectsGrid;
			InitializeComponent();
			ValuationDeclarationMethodTwoToSixUserControl.ChangeBindingToMessageSendingObject();
			MethodTwoToThreeUserControl.ChangeBindingToMessageSendingObject();
			MethodFourUserControl.ChangeBindingToMessageSendingObject();
			MethodFiveToSixUserControl.ChangeBindingToMessageSendingObject();
			QuestionGroupBoxUserControl.ChangeBindingToMessageSendingObject();
			PriceGroupBoxUserControl.ChangeBindingToMessageSendingObject();
		}
		readonly ZGrid messageSendingObjectsGrid;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			UpdateValuationDeclarationVisibility();
		}

		public override void SetDataBinding(object dataSource, string dataMember)
		{
			base.SetDataBinding(dataSource, dataMember);

			var listManager = messageSendingObjectsGrid?.ListManager;
			if (listManager != null)
			{
				listManager.CurrentChanged -= MessageSendingObjectsGrid_CurrentFocusedRowChanged;
				listManager.CurrentChanged += MessageSendingObjectsGrid_CurrentFocusedRowChanged;
			}
		}

		void MessageSendingObjectsGrid_CurrentFocusedRowChanged(object sender, EventArgs e)
		{
			UpdateValuationDeclarationVisibility();
		}

		void UpdateValuationDeclarationVisibility()
		{
			if (CurrentMessageSendingObject != null)
			{
				var valuationCode = CurrentMessageSendingObject.ValuationCode;
				var isValuationMethodDefaultOrA = valuationCode.IsEmpty || valuationCode == ValuationCodeList.Codes.MethodOne;
				QuestionTabPage.TabVisible = isValuationMethodDefaultOrA;
				PriceTabPage.TabVisible = isValuationMethodDefaultOrA;

				MethodTwoToSixTabPage.TabVisible = !isValuationMethodDefaultOrA;
				MethodTwoToThreeTabPage.TabVisible = false;
				MethodFourTabPage.TabVisible = false;
				MethodFiveToSixTabPage.TabVisible = false;

				switch (valuationCode)
				{
					case ValuationCodeList.Codes.MethodTwo:
					case ValuationCodeList.Codes.MethodThree:
						MethodTwoToThreeTabPage.TabVisible = true;
						break;
					case ValuationCodeList.Codes.MethodFourA:
					case ValuationCodeList.Codes.MethodFourB:
						MethodFourTabPage.TabVisible = true;
						break;
					case ValuationCodeList.Codes.MethodFive:
					case ValuationCodeList.Codes.MethodSix:
						MethodFiveToSixTabPage.TabVisible = true;
						break;
				}
			}
		}

		ValuationDeclarationMessageSendingObject CurrentMessageSendingObject
		{
			get
			{
				ValuationDeclarationMessageSendingObject result = null;
				if (messageSendingObjectsGrid != null)
				{
					if (messageSendingObjectsGrid.ListManager != null)
					{
						result = (ValuationDeclarationMessageSendingObject)messageSendingObjectsGrid.ListManager.GetCurrent();
					}
				}
				return result;
			}
		}
	}
}
