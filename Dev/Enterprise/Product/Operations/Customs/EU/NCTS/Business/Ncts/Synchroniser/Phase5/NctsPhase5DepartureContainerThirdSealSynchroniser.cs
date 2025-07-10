using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.NCTS.Business
{
	public class NctsPhase5DepartureContainerThirdSealSynchroniser : BaseSynchroniser
	{
		readonly NctsDepartureHeaderContainer destination;
		readonly ForwardingContainer source;

		public NctsPhase5DepartureContainerThirdSealSynchroniser(NctsDepartureHeaderContainer destination, ForwardingContainer source)
		{
			this.destination = destination;
			this.source = source;
		}

		protected override void HookEvents()
		{
			source.JC_Additional2SealNumInfo.ValueChanged -= JC_Additional2SealNumInfo_ValueChanged;
			source.JC_Additional2SealNumInfo.ValueChanged += JC_Additional2SealNumInfo_ValueChanged;
		}

		protected override void UnHookEvents()
		{
			source.JC_Additional2SealNumInfo.ValueChanged -= JC_Additional2SealNumInfo_ValueChanged;
		}

		void JC_Additional2SealNumInfo_ValueChanged(object sender, EventArgs e)
		{
			if (e is InfoEventArgs ve)
			{
				if (!lastValue.HasValue || !ve.Info.Value.Equals(lastValue.Value))
				{
					Synchronise();
				}

				lastValue = (ZString)ve.Info.Value;
			}
		}
		ZString? lastValue;

		protected override void OnDetectEnabledChanged()
		{
		}

		protected override void ForceSynchronise()
		{
			if (IsEnabled)
			{
				var thirdSealNum = source.JC_Additional2SealNum;
				if (thirdSealNum.IsEmpty)
				{
					destination.AdditionalSeals.RemoveAndDeleteAll();
					destinationSeal = null;
				}
				else
				{
					if (destinationSeal == null)
					{
						destinationSeal = destination.AdditionalSeals.Cast<CusSeal>().FirstOrDefault(x => x.BK_SealNumber == thirdSealNum);
						if (destinationSeal == null)
						{
							destinationSeal = destination.AdditionalSeals.AddNew();
							destinationSeal.BK_SealNumber = thirdSealNum;
						}
					}
					else
					{
						destinationSeal.BK_SealNumber = thirdSealNum;
					}

					var destinationSealPK = destinationSeal.PK;
					destination.AdditionalSeals.Where(x => x.PK != destinationSealPK).DeleteAll();
				}
			}
		}

		CusSeal destinationSeal;

		protected override void OnSynchronised()
		{
			base.OnSynchronised();
			SetBK_SealNumberReadOnly();
		}

		protected override void OnEnabledChanged()
		{
			base.OnEnabledChanged();
			SetBK_SealNumberReadOnly();
		}

		void SetBK_SealNumberReadOnly()
		{
			if (destinationSeal is ISynchroniserReadOnlyMembersProvider provider)
			{
				if (IsEnabled)
				{
					if (!provider.SynchroniserReadOnlyMembers.Contains(nameof(Customs.Business.CusSeal.BK_SealNumber)))
					{
						provider.SynchroniserReadOnlyMembers.Add(nameof(Customs.Business.CusSeal.BK_SealNumber));
					}
				}
				else
				{
					provider.SynchroniserReadOnlyMembers.Remove(nameof(Customs.Business.CusSeal.BK_SealNumber));
				}
			}
		}
	}
}
