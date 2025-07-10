using System.Data;
using System.Globalization;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CommissionPeriodCollection : RegistryBusinessObjectCollection, IRegistryCollectionToTVP
	{
		public CommissionPeriodCollection()
			: base(null, OrgCommissionAgreementRecipientRateSchema.CAT_CommissionPeriod.MaxLength)
		{
		}

		public new CommissionPeriod this[int i]
		{
			get { return (CommissionPeriod)base[i]; }
		}

		#region Add

		public new CommissionPeriod AddNew()
		{
			return (CommissionPeriod)base.AddNew();
		}

		public CommissionPeriod AddNew(ZString code, MultilingualString description, ZInt start, ZInt end)
		{
			var result = AddNew();
			using (result.SuspendSettingHasChanges())
			using (result.GetValidationSuspender())
			{
				result.Code = code;
				result.Description = description;
				result.Start = start;
				result.End = end;
			}

			return result;
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CommissionPeriod();
		}

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((CommissionPeriod)child).IsEnabled = true;
		}

		#endregion

		#region Find

		public new CommissionPeriod FindByCode(string code)
		{
			return (CommissionPeriod)base.FindByCode(code);
		}

		#endregion

		#region Clone

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CommissionPeriodCollection();
		}

		#endregion

		#region GetEnabledCodeDescriptionPairList

		public ReadOnlyCodeDescriptionPairList GetEnabledCodeDescriptionPairList()
		{
			var result = new CodeDescriptionPairList();
			foreach (CommissionPeriod element in this)
			{
				if (element.IsEnabled)
				{
					result.AddPair(element.Code, element.Description);
				}
			}
			return result;
		}

		#endregion

		#region IRegistryCollectionToTVP

		public string TVPType => "TVP_CommissionPeriod";

		public DataTable CreateDataTable()
		{
			var table = new DataTable();

			table.Locale = CultureInfo.InvariantCulture;
			table.Columns.Add((NoResString)"Code", typeof(string));
			table.Columns.Add((NoResString)"Description", typeof(string));
			table.Columns.Add((NoResString)"Start", typeof(int));
			table.Columns.Add((NoResString)"End", typeof(int));
			table.Columns.Add("IsEnabled", typeof(bool));

			foreach (CommissionPeriod period in this)
			{
				table.Rows.Add(
					new object[]
					{
						(string)period.Code,
						(string)period.Description,
						(int)period.Start,
						(int)period.End,
						(bool)period.IsEnabled
					});
			}
			return table;
		}

		#endregion
	}
}
