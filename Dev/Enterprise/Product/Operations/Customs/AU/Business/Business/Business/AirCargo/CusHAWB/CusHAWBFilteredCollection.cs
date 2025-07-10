using System;
using CargoWise.Types;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class CusHAWBFilteredCollection : Customs.Business.CusHAWBFilteredCollection
	{
		public CusHAWBFilteredCollection(CusMAWB mawb)
			: base(mawb)
		{
		}

		public new CusHAWB this[int i]
		{
			get
			{
				return (CusHAWB)Elements[i];
			}
		}

		public new CusHAWB AddNew()
		{
			return (CusHAWB)base.AddNew();
		}

		public new CusHAWB AddNew(Type bizOType)
		{
			return (CusHAWB)base.AddNew(bizOType);
		}

		#region Implementation

		protected override bool IsCustomsCargoStatusMatching(Customs.Business.CusHAWB hawb, ZString status)
		{
			bool result;

			switch (status)
			{
				case CMRConsolidatedCargoStatuses.Filter.Codes.NotClear:
					result = true;
					foreach (CodeDescriptionPair clearStatus in CMRConsolidatedCargoStatuses.AllClearStatus)
					{
						if (base.IsCustomsCargoStatusMatching(hawb, clearStatus.Code))
						{
							result = false;
							break;
						}
					}
					break;
				case CMRConsolidatedCargoStatuses.Filter.Codes.ClearOrConditional:
					result =
						base.IsCustomsCargoStatusMatching(hawb, CMRConsolidatedCargoStatuses.Codes.ClearCargoIsFreeOfAnyImpedimentsAndMayBeReleased) ||
						base.IsCustomsCargoStatusMatching(hawb, CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation);
					break;
				case CMRConsolidatedCargoStatuses.Filter.Codes.HeldOrConditional:
					result =
						base.IsCustomsCargoStatusMatching(hawb, CMRConsolidatedCargoStatuses.Codes.HeldCargoIsHeldUnderCustomsControl) ||
						base.IsCustomsCargoStatusMatching(hawb, CMRConsolidatedCargoStatuses.Codes.CondclearCargoCanBeReleasedIntoHomeConsumptionSubjectToConditionSTheseConditionsAreProvidedInSupplementaryInformation);
					break;
				default:
					result = base.IsCustomsCargoStatusMatching(hawb, status);
					break;
			}

			return result;
		}

		protected override bool IsCustomsMessageStatusMatching(Customs.Business.CusHAWB hawb, ZString status)
		{
			bool result;

			switch (status)
			{
				case CMRBaseStatuses.Codes.NotSent:
					result =
						base.IsCustomsMessageStatusMatching(hawb, status) ||
						base.IsCustomsMessageStatusMatching(hawb, ZString.Empty);
					break;
				default:
					result = base.IsCustomsMessageStatusMatching(hawb, status);
					break;
			}

			return result;
		}

		#endregion // Implementation
	}
}
