using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISPremisesIdAndProcessingType : NonPersistentBusinessObject, IAQISUniqueCodeForSort
	{
		public AQISPremisesIdAndProcessingType(BusinessObjectFactory factory, JobDeclaration declaration)
			: base(factory)
		{
			this.Declaration = declaration;
		}

		#region Premises Id

		[MaxLength(10)]
		[List(nameof(Lookups) + "." + nameof(AQISPremisesIdAndProcessingTypeLookups.AQISPremisesIdList))]
		public ZString PremisesId
		{
			get { return fPremisesId; }
			set
			{
				if (fPremisesId != value)
				{
					CheckMaximumLength(PremisesIdInfo, value);
					SetNonPersistentPropertyValue(PremisesIdInfo, ref fPremisesId, value);
					Validation.ValidatePremisesId();
				}
			}
		}
		ZString fPremisesId;

		public ZPropertyInfo PremisesIdInfo
		{
			get { return GetZPropertyInfo(nameof(PremisesId)); }
		}

		#endregion

		#region Processing Type

		[MaxLength(10)]
		[List(nameof(Lookups) + "." + nameof(AQISPremisesIdAndProcessingTypeLookups.AQISProcessingTypeList))]
		public ZString ProcessingType
		{
			get { return fProcessingType; }
			set
			{
				if (fProcessingType != value)
				{
					CheckMaximumLength(ProcessingTypeInfo, value);
					SetNonPersistentPropertyValue(ProcessingTypeInfo, ref fProcessingType, value);
					Validation.ValidateProcessingType();
				}
			}
		}
		ZString fProcessingType;

		public ZPropertyInfo ProcessingTypeInfo
		{
			get { return GetZPropertyInfo(nameof(ProcessingType)); }
		}

		#endregion

		#region Lookups

		public AQISPremisesIdAndProcessingTypeLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new AQISPremisesIdAndProcessingTypeLookups(this);
				}

				return fLookups;
			}
		}
		AQISPremisesIdAndProcessingTypeLookups fLookups;

		#endregion

		#region Validation

		public AQISPremisesIdAndProcessingTypeValidation Validation
		{
			get { return new AQISPremisesIdAndProcessingTypeValidation(this); }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		public readonly JobDeclaration Declaration;

		#region IAQISUniqueCodeForSort Members

		ZString[] IAQISUniqueCodeForSort.CodesToSortBy
		{
			get
			{
				return new ZString[] { ProcessingType, PremisesId };
			}
		}

#if DEBUG

		ZPropertyInfo[] IAQISUniqueCodeForSort.CodeInfosToSortByForTestingOnly
		{
			get
			{
				return new ZPropertyInfo[] { ProcessingTypeInfo, PremisesIdInfo };
			}
		}

#endif
		#endregion
	}
}
