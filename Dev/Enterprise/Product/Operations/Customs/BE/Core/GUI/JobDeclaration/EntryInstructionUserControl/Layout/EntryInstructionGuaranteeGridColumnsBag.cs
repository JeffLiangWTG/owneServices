using System;
using System.Windows.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.BE.GUI
{
	public sealed class EntryInstructionGuaranteeGridColumnsBag
	{
		public static EntryInstructionGuaranteeGridColumnsBag Instance => instance ?? (instance = new EntryInstructionGuaranteeGridColumnsBag());

		[ThreadStatic]
		static EntryInstructionGuaranteeGridColumnsBag instance;

		public EntryInstructionGuaranteeGridColumnsBag()
		{
			BondTypeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(CusBondDetail.Schema.PW_BondType, 40,
				c =>
				{
					c.IsMandatory = true;
					c.CaptionResourceString = Res.GetData("8A2822F9-DD39-4FF6-8FA0-649C9CA1E6C4", "[UCC 8/2] Type");
				});
			BondNumberMultiControlColumn = new GridColumnReference<ZMultiControlColumnStyleInfo>(CusBondDetail.Schema.PW_BondNumber, 120,
				c =>
				{
					c.BindToDecimalPlaces = null;
					c.FieldTypeColumnName = "ReferenceNumberFieldType";
					c.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Customs.Guarantees;
					c.CaptionResourceString = Res.GetData("8A2822F9-DD39-4FF6-8FA0-649C9CA1E6C8", "[UCC 8/3] Reference");
				});
			BondNumber2TextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(CusBondDetail.Schema.PW_BondNumber2, 120);
			PasswordBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(CusBondDetail.Schema.PW_Password, 120,
				c =>
				{
					c.TextAlign = HorizontalAlignment.Right;
					c.PasswordChar = '*';
				});
			HolderIdentificationTextBoxColumn = new GridColumnReference<ZTextBoxColumnStyleInfo>(CusBondDetail.Schema.PW_HolderIdentification, 120);
			NkCurrencyCodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(CusBondDetail.Schema.PW_RX_NKCurrency, 80);
			BondAmountCalcEditColumn = new GridColumnReference<ZCalcEditColumnStyleInfo>(CusBondDetail.Schema.PW_BondAmount, 80,
				c =>
				{
					c.BindToDecimalPlaces = "GuaranteeBondAmountDecimalPlaces";
					c.Decimals = 2;
					c.MaxValue = 0;
				});

			BondFiledPortCodeFindBoxColumn = new GridColumnReference<ZCodeFindBoxColumnStyleInfo>(CusBondDetail.Schema.PW_BondFiledPort, 100);
			SuretyCodeDropEditColumn = new GridColumnReference<ZDropEditColumnStyleInfo>(CusBondDetail.Schema.PW_SuretyCode, 100);
		}

		public IGridColumnReference BondTypeDropEditColumn { get; }

		public IGridColumnReference BondNumberMultiControlColumn { get; }

		public IGridColumnReference BondNumber2TextBoxColumn { get; }

		public IGridColumnReference PasswordBoxColumn { get; }

		public IGridColumnReference HolderIdentificationTextBoxColumn { get; }

		public IGridColumnReference NkCurrencyCodeFindBoxColumn { get; }

		public IGridColumnReference BondAmountCalcEditColumn { get; }

		public IGridColumnReference BondFiledPortCodeFindBoxColumn { get; }

		public IGridColumnReference SuretyCodeDropEditColumn { get; }
	}
}
