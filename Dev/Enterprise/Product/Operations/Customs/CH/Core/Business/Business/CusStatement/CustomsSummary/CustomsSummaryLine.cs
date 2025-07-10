using System;
using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using CargoWiseOne.ResourceStrings;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.CH.Business;

public sealed class CustomsSummaryLine : BaseCusStatementLine
{
	public new class Schema : AutoCusStatementLine.Schema
	{
		public const string ProcessDate = nameof(CustomsSummaryLine.ProcessDate);
	}

	public CustomsSummaryLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	new CustomsSummaryHeader StatementHeader => statementHeader ??= Factory.Load<CustomsSummaryHeader>(B3_B2);
	CustomsSummaryHeader statementHeader;

	[RelatedBusinessObject(nameof(StatementHeader))]
	public override ZGuid B3_B2 { get => base.B3_B2; set => base.B3_B2 = value; }

	[ResourceStringData("CH.CustomsSummaryLine|B3_EntryNum", Caption = "Entry Number")]
	public override ZString B3_EntryNum { get => base.B3_EntryNum; set => base.B3_EntryNum = value; }

	[ResourceStringData("CH.CustomsSummaryLine|B3_BrokerReference", Caption = "Trader Reference")]
	public override ZString B3_BrokerReference { get => base.B3_BrokerReference; set => base.B3_BrokerReference = value; }

	[ResourceStringData("CH.CustomsSummaryLine|B3_Status", Caption = "eVV Document Received Status")]
	public override ZString B3_Status { get => base.B3_Status; set => base.B3_Status = value; }

	[ResourceStringData("CH.CustomsSummaryLine|StatusDescription", Caption = "eVV Document Received Status Desc.")]
	public ZString StatusDescription => Factory.GetCachedValue<BordereauReceivedStatusList>().GetDescriptionFromCode(B3_Status);

	public CustomsSummaryLineCharge LineCharge => lineCharge ??= LoadOrCreateLineCharge(true);
	CustomsSummaryLineCharge lineCharge;

	CustomsSummaryLineCharge LoadOrCreateLineCharge(bool createIfNotExists)
	{
		lineCharge = Factory.LoadTop1<CustomsSummaryLineCharge>(new ZQuery(CusStatementLineChargeSchema.B4_B3, PK));
		if (lineCharge == null && createIfNotExists)
		{
			lineCharge = Factory.New<CustomsSummaryLineCharge>();
			lineCharge.B4_B3 = PK;
		}
		return lineCharge;
	}

	[ResourceStringData("CH.CustomsSummaryLine|StatementNumber", Caption = "Summary Number")]
	public ZString StatementNumber => StatementHeader.B2_StatementNumber;

	[ResourceStringData("CH.CustomsSummaryLine|ProcessDate", Caption = "Summary Date")]
	public ZDateTime ProcessDate => StatementHeader.B2_ProcessDate;

	[ResourceStringData("CH.CustomsSummaryLine|B2_AccountNo", Caption = "Account Number")]
	public ZString AccountNo => StatementHeader.B2_AccountNo;

	[ResourceStringData("CH.CustomsSummaryLine|ChargeType", Caption = "eVV Document Type")]
	public ZString ChargeType => LineCharge.B4_ChargeType;

	[ResourceStringData("CH.CustomsSummaryLine|ChargeTypeDescription", Caption = "eVV Document Type Desc.")]
	public ZString ChargeTypeDescription => Factory.GetCachedValue<BordereauChargeTypeList>().GetDescriptionFromCode(ChargeType);

	[ResourceStringData("CH.CustomsSummaryLine|ChargeAmount", Caption = "Amount")]
	public ZDecimal ChargeAmount => LineCharge.B4_ChargeAmount;

	[ResourceStringData("CH.CustomsSummaryLineCharge|B4_ReferenceNumber", Caption = "Customs Office Number")]
	public ZString ReferenceNumber => LineCharge.B4_ReferenceNumber;

	protected override EnterpriseBusinessObjectFetchStrategy GetFetchStrategyCore() => new CustomsSummaryLineFetchStrategy(this);

	protected override void OnSavingForDelete()
	{
		base.OnSavingForDelete();
		var lineCharge = LoadOrCreateLineCharge(false);
		if (lineCharge != null)
		{
			lineCharge.Delete();
		}
	}

	public new class Loader : BusinessObject.Loader
	{
		public Loader(BusinessObjectFactory factory) : base(factory)
		{
		}

		protected override Type GetTypeOfBusinessObjectToLoad() => typeof(CustomsSummaryLine);

		public CustomsSummaryLine LoadCustomsSummaryLine(string entryNum, string chargeType)
		{
			var chargeQuery = new ZDBOnlySubQuery(typeof(CustomsSummaryLineCharge), CusStatementLineChargeSchema.B4_B3);
			chargeQuery.AddToFilter(CusStatementLineChargeSchema.B4_ChargeType, chargeType);

			var lineQuery = new ZDBOnlyQuery(typeof(CustomsSummaryLine));
			lineQuery.AddToFilter(CusStatementLineSchema.B3_EntryNum, entryNum);
			lineQuery.AddSubQuery(chargeQuery, JoinCondition.And);

			return Factory.LoadTop1<CustomsSummaryLine>(lineQuery);
		}
	}
}
