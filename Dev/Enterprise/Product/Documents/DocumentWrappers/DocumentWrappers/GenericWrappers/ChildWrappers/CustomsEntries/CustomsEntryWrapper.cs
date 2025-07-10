using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentWrappers.GenericWrappers.Base;

namespace Enterprise.DocumentWrappers.GenericWrappers
{
	[DefaultField("EntryNumber"), WrapperTypeName("CustomsEntry")]
	public abstract class CustomsEntryWrapper : GenericWrapper, ISearcheableCusEntryNumber
	{
		protected CustomsEntryWrapper(BusinessObject objectToWrap, BusinessObjectFactory factory)
			: base(objectToWrap, factory)
		{
		}

		#region Properties

		#region Type

		public CodeAndDescriptionWrapper EntryType
		{
			get { return fEntryType ?? (fEntryType = GetEntryType()); }
		}
		CodeAndDescriptionWrapper fEntryType;

		protected abstract CodeAndDescriptionWrapper GetEntryType();

		#endregion

		#region Number

		public ZString EntryNumber
		{
			get { return GetEntryNumber(); }
		}

		protected abstract ZString GetEntryNumber();

		#endregion

		#region Information

		public ZString Information
		{
			get { return GetInformation(); }
		}

		protected abstract ZString GetInformation();

		#endregion

		#region Issue Date

		public ZDateTime IssueDate
		{
			get { return GetIssueDate(); }
		}

		protected abstract ZDateTime GetIssueDate();

		#endregion

		#region Category

		public ZString EntryCategory
		{
			get { return GetEntryCategory(); }
		}

		protected virtual ZString GetEntryCategory()
		{
			return ZString.Empty;
		}

		#endregion

		#region Country

		public ZString Country
		{
			get { return GetCountry(); }
		}

		protected virtual ZString GetCountry()
		{
			return ZString.Empty;
		}

		#endregion

		#endregion

		ZString ISearcheableCusEntryNumber.CE_Category
		{
			get { return EntryCategory; }
		}

		ZString ISearcheableCusEntryNumber.CE_EntryNum
		{
			get { return EntryNumber; }
		}

		ZString ISearcheableCusEntryNumber.CE_EntryType
		{
			get { return EntryType != null ? EntryType.Code : ZString.Empty; }
		}

		ZString ISearcheableCusEntryNumber.CE_RN_NKCountryCode
		{
			get { return Country; }
		}
	}
}
