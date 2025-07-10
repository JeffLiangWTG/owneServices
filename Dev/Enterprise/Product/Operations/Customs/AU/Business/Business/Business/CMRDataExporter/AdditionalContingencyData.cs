using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AdditionalContingencyData : NonPersistentBusinessObject, IObsoleteValidation
	{
		public AdditionalContingencyData(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Properties

		#region Origin premise

		public bool IsOriginPremiseReadOnly { get; set; }

		[ReadOnlyMember(nameof(IsOriginPremiseReadOnly))]
		[List(nameof(OriginPremiseList))]
		[MaxLength(5)]
		public ZString OriginPremise
		{
			get { return originPremise; }
			set
			{
				CheckMaximumLength(OriginPremiseInfo, value);
				SetNonPersistentPropertyValue(OriginPremiseInfo, ref originPremise, value);
				if (!IsValidationSuspended)
				{
					ValidateOriginPremise();
				}
			}
		}
		ZString originPremise;

		public ZPropertyInfo OriginPremiseInfo
		{
			get { return GetZPropertyInfo(nameof(OriginPremise)); }
		}

		public CMREstablishmentCodesCollection OriginPremiseList
		{
			get { return new CMREstablishmentCodesCollection(Factory); }
		}

		#endregion

		#endregion

		#region Validation

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			ValidateOriginPremise();
		}

		#region Origin Premise

		public void ValidateOriginPremise()
		{
			OriginPremiseInfo.ClearAllNotifications();
			if (!OriginPremiseInfo.ReadOnly)
			{
				MandatoryValidation.CheckEntered(OriginPremiseInfo);
			}
		}

		#endregion

		#endregion
	}
}
