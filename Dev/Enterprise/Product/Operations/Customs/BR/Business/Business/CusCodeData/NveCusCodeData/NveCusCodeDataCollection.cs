using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.BR;

namespace Enterprise.Customs.BR.Business
{
	public class NveCusCodeDataCollection : CusCodeDataCollection<NveCusCodeData>
	{
		public NveCusCodeDataCollection(JobComInvoiceLine parent)
			: base(parent, CusCodeDataTypeList.Codes.NVE)
		{
		}

		public NveCusCodeDataCollection(CusClassPartPivot parent)
			: base(parent, CusCodeDataTypeList.Codes.NVE)
		{
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;

		public override bool ReadOnly => base.ReadOnly || (Master is JobComInvoiceLine parent && parent.HasLinkedInvoiceLine);

		internal void RebuildFromCharacteristics()
		{
			this.Cast<NveCusCodeData>().ForEach(x => x.TariffCharacteristic = null);
			IEnumerable<Universal.RefCusTariffBRCharacteristic> characteristics = null;

			if (Master is JobComInvoiceLine invoiceLine && (invoiceLine.IsImportSiscomex || invoiceLine.IsImportLicense))
			{
				characteristics = invoiceLine.UniversalTariff?.GetTariffNVECharacteristics(invoiceLine.EffectiveAssessmentDate);
			}
			else if (Master is CusClassPartPivot pivot && pivot.IsImportClassification)
			{
				characteristics = pivot.UniversalTariff?.GetTariffNVECharacteristics(ZDateTime.Today);
			}

			if (characteristics?.Any() ?? false)
			{
				foreach (var characteristic in characteristics.Where(x => x.ZB1_IsMandatory).OrderBy(x => x.ZB1_Code))
				{
					var nve = GetFirstElementHaving(characteristic.ZB1_Code) ?? AddNew();

					using (nve.GetValidationSuspender())
					using (nve.SuspendSettingHasChanges())
					{
						nve.CY_Code = characteristic.ZB1_Code;
						nve.TariffCharacteristic = characteristic;
					}
				}
			}
			RemoveNullCharacteristics();
		}

		void RemoveNullCharacteristics()
		{
			foreach (var nve in this.Cast<NveCusCodeData>().Where(x => x.TariffCharacteristic == null).ToArray())
			{
				RemoveAndDelete(nve);
			}
		}

		public void CopyDataFrom(NveCusCodeDataCollection collectionToCopy)
		{
			foreach (NveCusCodeData nveToClone in collectionToCopy)
			{
				var nve = GetFirstElementHaving(nveToClone.CY_Code);
				if (nve != null)
				{
					using (nve.GetValidationSuspender())
					using (nve.SuspendSettingHasChanges())
					{
						nve.CY_Data = nveToClone.CY_Data;
					}
				}
			}
		}
	}
}
