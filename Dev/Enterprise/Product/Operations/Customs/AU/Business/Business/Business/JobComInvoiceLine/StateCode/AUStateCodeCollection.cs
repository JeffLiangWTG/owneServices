using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public sealed class AUStateCodeCollection : NonPersistentBusinessObjectCollection<AUStateCode>
	{
		public AUStateCodeCollection(JobComInvoiceLine invoiceLine, BusinessObjectFactory factory)
			: base(factory)
		{
			this.invoiceLine = invoiceLine;
			LoadCollection();
		}

		public JobComInvoiceLine Parent
		{
			get { return invoiceLine; }
		}
		readonly JobComInvoiceLine invoiceLine;

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AUStateCode(Parent, Factory);
		}

		void LoadCollection()
		{
			if (invoiceLine != null && invoiceLine.AddInfo != null && !invoiceLine.AddInfo.ZA_AUState_Hidden.IsEmpty)
			{
				SplitAUStateCodeElements(invoiceLine.AddInfo.ZA_AUState_Hidden);
			}
		}

		void SplitAUStateCodeElements(string addInfoStringToSplit)
		{
			if (this.Count > 0)
			{
				RemoveAll();
			}

			string[] spiltValues = addInfoStringToSplit.Split(',');

			foreach (ZString value in spiltValues)
			{
				if (!value.IsEmpty)
				{
					AUStateCode newBizObj = this.AddNew();
					using (newBizObj.SuspendSettingHasChanges())
					{
						if (value.Length <= newBizObj.CodeInfo.MaxLength)
						{
							newBizObj.Code = value;
						}
						else
						{
							this.RemoveAndDelete(newBizObj);
						}
					}
				}
			}
		}

		public void ReBuildAUState()
		{
			ZStringBuilder result = new ZStringBuilder();

			foreach (AUStateCode bizObj in this)
			{
				result.Append(bizObj.Code + ",");
			}
			invoiceLine.AddInfo.ZA_AUState_Hidden = new ZString(result.ToString()).TrimEndIncludingWhiteSpace(',');
		}
	}
}
