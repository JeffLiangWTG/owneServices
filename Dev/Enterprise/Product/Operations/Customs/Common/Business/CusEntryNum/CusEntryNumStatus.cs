using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.Common
{
	public class CusEntryNumStatus : NonPersistentBusinessObject, IObsoleteValidation
	{
		public CusEntryNumStatus(BusinessObject parent, CodeDescriptionPairList list, ZString defaultValue, string entryType, string countryCode)
		{
			fParent = parent;
			fList = list;
			this.defaultValue = defaultValue;
			fEntryType = entryType;
			this.countryCode = countryCode;
		}

		#region Schema

		public abstract class Schema
		{
			public const string Code = "Code";
			public const string Description = "Description";
		}

		#endregion

		#region Properties

		#region Description

		[ReadOnly(true)]
		public ZString Description
		{
			get { return fList.GetDescriptionFromCode(Code); }
		}

		public ZPropertyInfo DescriptionInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}

		#endregion

		#region Code

		[ReadOnly(true)]
		[CargoWise.ComponentModel.MaxLength(3)]
		public ZString Code
		{
			get { return Status.CE_EntryStatus; }
			set
			{
				if (Status != null)
				{
					if (value == defaultValue && !EntryNumberShouldNotBeDeletedIfSetBackToDefault(Status))
					{
						Status.Delete();
						fStatus = null;
					}
					else
					{
						Status.CE_EntryStatus = value;
					}
				}
				CodeInfo.RefreshBinding();
			}
		}

		bool EntryNumberShouldNotBeDeletedIfSetBackToDefault(CusEntryNumber status)
		{
			foreach (string numType in EntryNumberTypesThatShouldNotBeDeleted)
			{
				if (status.CE_EntryType == numType)
				{
					return true;
				}
			}
			return false;
		}

		string[] EntryNumberTypesThatShouldNotBeDeleted
		{
			get
			{
				return new string[]
			{
			  CusEntryNumber.EntryType.UnderbondStatus,
			  CusEntryNumber.EntryType.OutturnStatus
			};
			}
		}

		public ZPropertyInfo CodeInfo
		{
			get { return GetZPropertyInfo(Schema.Code); }
		}

		#endregion

		#region Status

		protected virtual CusEntryNumber StatusCore
		{
			get
			{
				if (fStatus == null || fStatus.IsDeleted)
				{
					fStatus = CusEntryNumber.Load(fParent, fEntryType, countryCode);
					if (fStatus == null)
					{
						fStatus = CusEntryNumber.New(fParent, fEntryType, countryCode);
						fStatus.CE_EntryStatus = defaultValue;
					}
				}
				return fStatus;
			}
		}

		public CusEntryNumber Status
		{
			get { return StatusCore; }
		}

		public void DeleteStatusIfDefaultValue()
		{
			var status = CusEntryNumber.Load(fParent, fEntryType, countryCode);
			if (status != null && status.CE_EntryStatus == defaultValue)
			{
				status.Delete();
			}
		}

		#endregion

		#endregion

		public CodeDescriptionPairList List
		{
			get { return fList; }
		}

		#region Implementation

		readonly BusinessObject fParent;
		readonly CodeDescriptionPairList fList;
		readonly string fEntryType;
		CusEntryNumber fStatus;
		readonly ZString defaultValue;
		readonly string countryCode;

		#endregion
	}
}
