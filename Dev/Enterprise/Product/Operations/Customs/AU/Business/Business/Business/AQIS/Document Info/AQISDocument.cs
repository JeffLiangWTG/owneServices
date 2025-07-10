using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Customs.AU.Declaration.Business
{
	public class AQISDocument : NonPersistentBusinessObject, IAQISUniqueCodeForSort
	{
		public AQISDocument(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		#region Type

		[MaxLength(10)]
		[List(nameof(Lookups) + "." + nameof(AQISDocumentLookups.AQISDocumentTypeList))]
		public ZString Type
		{
			get { return fType; }
			set
			{
				if (fType != value)
				{
					CheckMaximumLength(TypeInfo, value);
					SetNonPersistentPropertyValue(TypeInfo, ref fType, value);
					Validation.ValidateType();
				}
			}
		}
		ZString fType;

		public ZPropertyInfo TypeInfo
		{
			get { return GetZPropertyInfo(nameof(Type)); }
		}

		#endregion

		#region Number

		[MaxLength(35)]
		public ZString Number
		{
			get { return fNumber.Replace("^", "/"); }
			set
			{
				if (fNumber != value)
				{
					CheckMaximumLength(NumberInfo, value);
					SetNonPersistentPropertyValue(NumberInfo, ref fNumber, value.Replace("/", "^"));
					Validation.ValidateNumber();
				}
			}
		}
		ZString fNumber;

		public ZPropertyInfo NumberInfo
		{
			get { return GetZPropertyInfo(nameof(Number)); }
		}

		public ZString RawNumber
		{
			get { return fNumber; }
		}

		#endregion

		#region Lookups

		public AQISDocumentLookups Lookups
		{
			get
			{
				if (fLookups == null)
				{
					fLookups = new AQISDocumentLookups(this);
				}

				return fLookups;
			}
		}
		AQISDocumentLookups fLookups;

		#endregion

		#region Validation

		public AQISDocumentValidation Validation
		{
			get { return new AQISDocumentValidation(this); }
		}

		protected override void RunPreSaveValidationCore()
		{
			base.RunPreSaveValidationCore();
			Validation.ValidateAll();
		}

		#endregion

		#region IAQISUniqueCodeForSort Members

		ZString[] IAQISUniqueCodeForSort.CodesToSortBy
		{
			get
			{
				return new ZString[] { Type, Number };
			}
		}

#if DEBUG

		ZPropertyInfo[] IAQISUniqueCodeForSort.CodeInfosToSortByForTestingOnly
		{
			get
			{
				return new ZPropertyInfo[] { TypeInfo, NumberInfo };
			}
		}

#endif

		#endregion
	}
}
