using CargoWise.Common;
using CargoWise.EntityFramework;

namespace Enterprise.UniversalDataBuss.Management.Matching
{
	public class PotentialMatch
	{
		internal PotentialMatch(BusinessObject targetData)
		{
			this.TargetData = Argument.NotNull(targetData, "BusinessObject targetData");
		}

		internal readonly BusinessObject TargetData;

		internal virtual string HumanReadableName
		{
			get { return TargetData.HumanReadableName; }
		}
	}

	class PotentialMatch<U, V> : PotentialMatch
		where U : BusinessObject
		where V : BusinessObject
	{
		internal PotentialMatch(U targetData, OuterTargetGetter<U, V> outerTargetGetter)
			: base(targetData)
		{
			this.outerTargetGetter = Argument.NotNull(outerTargetGetter, "OuterTargetGetter<U, V> outerTargetGetter");
		}
		readonly OuterTargetGetter<U, V> outerTargetGetter;

		V outerBO;
		internal V OuterTarget
		{
			get { return outerBO ?? (outerBO = outerTargetGetter((U)TargetData)); }
		}

		internal override string HumanReadableName
		{
			get { return OuterTarget.HumanReadableName; }
		}
	}

	delegate V OuterTargetGetter<U, V>(U innerBO)
		where U : BusinessObject
		where V : BusinessObject;
}
