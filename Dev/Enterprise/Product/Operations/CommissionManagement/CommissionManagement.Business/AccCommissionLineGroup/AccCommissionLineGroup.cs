using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.CommissionManagement.Business
{
	public class AccCommissionLineGroup : AutoAccCommissionLineGroup, IAccCommissionLineGroup
	{
		public AccCommissionLineGroup(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		#region Properties

		#region CommissionHeader

		[RelatedBusinessObject("CommissionHeader")]
		[List("Lookups.CommissionHeaders")]
		public override ZGuid CLG_CH0
		{
			get { return base.CLG_CH0; }
			set { base.CLG_CH0 = value; }
		}

		public virtual AccCommissionHeader CommissionHeader
		{
			get { return Factory.Load<AccCommissionHeader>(CLG_CH0); }
		}

		#endregion

		#region TotalCommissionableAmount

		[DecimalPlaces(nameof(CommissionDecimals))]
		public override ZDecimal CLG_TotalCommissionableAmount
		{
			get => base.CLG_TotalCommissionableAmount;
			set => base.CLG_TotalCommissionableAmount = value;
		}

		#endregion

		#region TransactionAmount

		[DecimalPlaces(nameof(TransactionDecimals))]
		public override ZDecimal CLG_TransactionAmount
		{
			get => base.CLG_TransactionAmount;
			set => base.CLG_TransactionAmount = value;
		}

		#endregion

		public int TransactionDecimals => TransactionCurrency != null ? TransactionCurrency.Decimals : GlbCompany.CurrentCompany.GetLocalDecimals();
		public int CommissionDecimals => CommissionCurrency != null ? CommissionCurrency.Decimals : GlbCompany.CurrentCompany.GetLocalDecimals();

		#endregion

		#region Override

		public void MarkAsOverriden(bool isAlreadyReversed = false)
		{
			CLG_TransactionAmount = 0;
			CLG_TotalCommissionableAmount = 0;
			foreach (var line in Lines.ToArray())
			{
				line.MarkAsOverriden(isAlreadyReversed);
			}
		}

		#endregion

		#region Related Business Objects

		public AccCommissionLineCollection Lines
		{
			get
			{
				if (lines == null)
				{
					lines = new AccCommissionLineCollection(this);
				}

				return lines;
			}
		}
		AccCommissionLineCollection lines;

		IEnumerable<IAccCommissionLine> IAccCommissionLineGroup.Lines => Lines.Cast<IAccCommissionLine>();

		#endregion

		#region Delete

		public override void Delete()
		{
			DeleteRelatedBusinessObjects();
			base.Delete();
		}

		void DeleteRelatedBusinessObjects()
		{
			Lines.DeleteAll();
		}

		#endregion
	}
}
