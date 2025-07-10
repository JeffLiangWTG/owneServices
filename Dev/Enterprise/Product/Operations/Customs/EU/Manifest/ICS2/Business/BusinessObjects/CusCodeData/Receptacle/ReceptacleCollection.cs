using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.EU.Manifest.ICS2.Business
{
	public class ReceptacleCollection : CusCodeDataCollection<Receptacle>, ISequenceNumberHeader
	{
		public ReceptacleCollection(BusinessObject master)
			: base(master, CusCodeDataTypeList.Codes.EUICS2Receptacle)
		{
			MaxCountValidationEnable(MaxNumberOfAllowedReceptacles, Res.GetString("D18B2B55-5F20-4039-9F0F-29FF4893FA2B", "A maximum of {0} records is allowed.", MaxNumberOfAllowedReceptacles));
		}

		public new ZString AsString
		{
			get => Factory.GetValue(ref asStringCached, () =>
			{
				return string.Join(",", this.Cast<CusCodeData>().OrderBy(x => x.CY_Order).Select(x => x.CY_Data).Where(x => !x.IsEmpty));
			});
			set
			{
				RemoveAndDeleteAll();
				foreach (string code in value.ToString().Replace(" ", ",").Split(',').Where(x => !string.IsNullOrWhiteSpace(x)))
				{
					var item = AddNew();
					item.CY_Data = new ZString(code).Trim();
				}
			}
		}
		CachedProperty<ZString> asStringCached;

		#region ISequenceNumberHeader

		public ShortSequenceNumberGenerator SequenceNumberCalculator
		{
			get { return sequenceNumberCalculator ?? (sequenceNumberCalculator = new ShortSequenceNumberGenerator(this)); }
		}
		ShortSequenceNumberGenerator sequenceNumberCalculator;

		IEnumerable<ISequenceNumberLine> ISequenceNumberHeader.Lines => this.Cast<ISequenceNumberLine>();

		#endregion

		const int MaxNumberOfAllowedReceptacles = 999;
	}
}
