
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class SACDialogBizo : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SACDialogBizo(ISACLiabilityQuestionProvider sAC) : base(((BusinessObject)sAC).Factory)
		{
			this.valueToCheck = sAC.GoodsValueInLocalCurrency;
			this.description = sAC.GoodsDescription;
			this.sACFlagInfo = sAC.SACFlagInfo;
		}

		#region Value

		public ZString Value
		{
			get
			{
				if (fValue.IsEmpty)
				{
					var deminimus = (int)UniversalReferenceHelper.GetDeminimus(new BusinessObjectFactory());
					if (Decider.IsValueOverTheScreenFreeValue)
					{
						fValue = "Over the screen value of $" + deminimus.ToString();
					}
					else
					{
						fValue = "Under the screen value of $" + deminimus.ToString();
					}
				}

				return fValue;
			}
		}
		ZString fValue;

		public ZPropertyInfo ValueInfo
		{
			get { return GetZPropertyInfo(nameof(Value)); }
		}

		#endregion

		#region Thesaurus

		public ZString Thesaurus
		{
			get
			{
				if (fThesaurus.IsEmpty)
				{
					if (Decider.StopPhrasesFoundInGoodsDescription.Any())
					{
						fThesaurus = string.Join(", ", Decider.StopPhrasesFoundInGoodsDescription);
					}
					else
					{
						fThesaurus = "No words found.";
					}
				}

				return fThesaurus;
			}
		}
		ZString fThesaurus;

		public ZPropertyInfo ThesaurusInfo
		{
			get { return GetZPropertyInfo(nameof(Thesaurus)); }
		}

		#endregion

		#region Set SAC Flag

		public void SetSACFlag(ZBool isSAC)
		{
			sACFlagInfo.Value = isSAC;
		}

		#endregion

		#region SAC Decider

		SACDecider Decider
		{
			get
			{
				if (fDecider == null)
				{
					fDecider = new SACDecider(Factory, valueToCheck, description);
				}

				return fDecider;
			}
		}
		SACDecider fDecider;

		#endregion

		#region SACQuestion

		public ZString SACQuestion
		{
			get
			{
				if (fSACQuestion.IsEmpty)
				{
					var deminimus = (int)UniversalReferenceHelper.GetDeminimus(new BusinessObjectFactory());
					string sACValue = deminimus.ToString();

					fSACQuestion =
						@"Checking this box means that the person sending the cargo report is also making a self assessed clearance declaration for the purposes of section 71 of the Customs Act. The self assessed clearance declaration made by checking this indicator declares that:" + System.Environment.NewLine + System.Environment.NewLine +
"* the value of the goods does not exceed $" + sACValue + @" (or other prescribed amount); and" + System.Environment.NewLine + System.Environment.NewLine +
"* the description of the goods does not include any word, term or description specified in the Thesaurus provided by Customs." + System.Environment.NewLine + System.Environment.NewLine +
"Do not check this if you cannot declare the above with certainty. A separately lodged self assessed clearance declaration can be made if you wish to provide more information in relation to the goods for Customs or Quarantine consideration.";
				}

				return fSACQuestion;
			}
		}

		ZString fSACQuestion;

		public ZPropertyInfo SACQuestionInfo
		{
			get { return GetZPropertyInfo(nameof(SACQuestion)); }
		}

		#endregion

		#region Implementation

		readonly ZDecimal valueToCheck;
		readonly ZString description;
		readonly ZPropertyInfo sACFlagInfo;

		#endregion
	}
}
