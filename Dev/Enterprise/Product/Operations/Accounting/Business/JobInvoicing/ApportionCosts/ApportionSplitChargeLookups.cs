using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using static System.FormattableString;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class ApportionSplitChargeLookups : BaseChargeLookups
	{
		public ApportionSplitChargeLookups(AutoJobCharge parent) : base(parent)
		{
			Parent = (ApportionSplitCharge)parent;
		}

		new ApportionSplitCharge Parent { get; }

		public CodeDescriptionPairList ApportionTargetJobNumbers
		{
			get
			{
				var result = new CodeDescriptionPairList();

				foreach (var target in Parent.ApportionTargets())
				{
					var invSupporter = target.InvoicingSupporter;
					result.AddPair(target.JobNumber, Invariant($"{invSupporter.Origin?.Code} - {invSupporter.Destination?.Code}"));
				}

				return result;
			}
		}
	}
}
