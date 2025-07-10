using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;

namespace Enterprise.Customs.CN.Business
{
	public class CIQProductQualificationCollection : CusSupportingInfoCollection<CIQProductQualification>
	{
		public CIQProductQualificationCollection(BusinessObject parent) : base(parent, Constants.CusSupportingInfoTypes.CIQProductQualification)
		{
		}

		public ZString MergeKey
		{
			get
			{
				var mergeKeys = this.Cast<CIQProductQualification>().Select(pq => pq.MergeKey);
				return ZString.Join("|", mergeKeys.OrderBy(k => k).ToArray());
			}
		}

		public bool IsProvidedAny(params string[] documentTypes)
		{
			return this.Cast<CIQProductQualification>().Any(x => documentTypes.Contains(x.CSI_Code.ToString()) && !x.CSI_ReferenceNumber.IsEmpty);
		}
	}
}
