using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Business
{
	public class EuOfficeCodeCollection : CusCodeDataCollection<EuOfficeCode>
	{
		public EuOfficeCodeCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.OfficeCode)
		{
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			var canDelete = DefaultPurposeCode.IsEmpty || Count > 1;

			if (canDelete)
			{
				if (!DefaultPurposeCode.IsEmpty)
				{
					// prevent removal of the last instance of the default code
					var office = (EuOfficeCode)elementToDelete;
					if (office != null && office.CY_Code == DefaultPurposeCode)
					{
						var codes = Find(e => e != null && e.CY_Code == DefaultPurposeCode);
						canDelete = codes.Count() > 1;
					}
				}
			}

			if (canDelete)
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}

		public ZString DefaultPurposeCode { get; set; }

		public void CreateDefaultEntry(ZBool suspendHasChanges)
		{
			if (Count == 0 && !DefaultPurposeCode.IsEmpty)
			{
				var office = AddNew();
				using (suspendHasChanges ? office.SuspendSettingHasChanges() : null)
				{
					office.CY_Code = DefaultPurposeCode;
				}
			}
		}

		public void DefaultMandatoryCustomsOffices()
		{
			var requirements = ((IEuOfficeCodeProvider)Master).CustomsOfficeRequirementHelper.OtherRequirements.ToArray();
			var allRequirementRoles = requirements.Select(x => x.OfficeRole);
			var officeCodesToDelete = this.OfType<EuOfficeCode>().Where(x => !allRequirementRoles.Contains(x.CY_Code)).ToArray();
			officeCodesToDelete.ForEach(x => RemoveAndDelete(x));

			var requirementsToAdd = requirements.Where(r => r.IsMandatory && this.Cast<EuOfficeCode>().All(o => o.CY_Code != r.OfficeRole)).Select(r => r.OfficeRole).ToArray();
			requirementsToAdd.ForEach(r => AddNew(r));
		}
	}
}
