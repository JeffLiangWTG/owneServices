using System;
using CargoWise.EntityFramework;
using Res = Enterprise.ZArchitecture.Business.Res;

namespace Enterprise.ZArchitecture
{
	public class BusinessObjectCreatedOrUpdatedNotification : BusinessObjectAfterSaveNotification
	{
		public BusinessObjectCreatedOrUpdatedNotification(BusinessObject bizObj)
			: base(bizObj, NotificationSubscriberType.BusinessObjectCreatedOrUpdated)
		{
			WasInDatabase = BusinessEntity.IsInDatabase;
			fDisplayMessageCore = String.Empty;
		}

		protected override string DisplayMessageCore
		{
			get
			{
				if (BusinessEntity != null && !BusinessEntity.IsDeleted)
				{
					fDisplayMessageCore = WasInDatabase ? Res.GetString("cb21b519-5326-4214-a7ca-e1c73e32f9b8", "{0} updated", BusinessEntity.HumanReadableName) : Res.GetString("e3eafc9c-73ce-4b7a-af58-ae2bbf283079", "{0} created", BusinessEntity.HumanReadableName);
				}
				return fDisplayMessageCore;
			}
		}
		string fDisplayMessageCore;

		protected override bool AllowBlankDisplayMessage
		{
			get { return (BusinessEntity != null && BusinessEntity.IsDeleted); }
		}

		public readonly bool WasInDatabase;

		public bool UpdateRecordCountOnlyWithoutMessage { get; set; }
	}
}
