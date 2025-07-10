using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business.FilterStrips
{
	public class FilterStripLayoutsHelperForWeb : FilterStripLayoutsHelper
	{
		#region Schema

		public abstract class Schema
		{
			public const string CurrentLayoutName = "CurrentLayoutName";
			public const string Layouts = "Layouts";
		}

		#endregion

		public FilterStripLayoutsHelperForWeb(FilterStripBusinessObject parent, IContactable loggedInUser)
		{
			Parent = parent;
			LoggedInUser = loggedInUser;
		}

		public FilterStripLayoutsHelperForWeb(FilterStripBusinessObject parent, IContactable loggedInUser, FilterLayoutCodePairRegistryItem registryItem) : this(parent, loggedInUser)
		{
			CurrentLayoutRegistryItem = registryItem;
		}
		public FilterLayoutCodePairRegistryItem CurrentLayoutRegistryItem { get; set; }

		#region CurrentLayout

		[BusinessObjectTestExclude]
		[MaxLength(StmModuleFilter.Schema.S9_FilterNameMaxLength + 2)]
		public ZString CurrentLayoutName
		{
			get { return CurrentLayout != null ? CurrentLayout.DisplayName : ZString.Empty; }
			set
			{
				var match = Layouts.ToArray().FirstOrDefault(item => item.Code == value);
				CurrentLayout = match != null ? (StmModuleFilter)match.PK : null;
			}
		}

		public ZPropertyInfo CurrentLayoutNameInfo
		{
			get { return GetZPropertyInfo(Schema.CurrentLayoutName); }
		}

		public StmModuleFilter CurrentLayout
		{
			get
			{
				if (!currentLayoutInitialized)
				{
					if (currentLayout == null)
					{
						currentLayout = Parent.LastUsedLayout;
					}
					currentLayoutInitialized = true;
				}
				if (currentLayout == null)
				{
					currentLayout = DefaultLayout;
				}
				return currentLayout;
			}
			set
			{
				var previousLayout = currentLayout;
				currentLayout = value;
				OnCurrentLayoutChanged(previousLayout, currentLayout);
				ResetIsCurrentLayoutDeleted();
				CurrentLayoutNameInfo.RefreshBinding();
			}
		}
		bool currentLayoutInitialized;
		StmModuleFilter currentLayout;

		public StmModuleFilter DefaultLayout
		{
			get;
			set;
		}

		#region Test
#if DEBUG
		public bool CurrentLayoutNameChanged_ForTest;
#endif
		#endregion

		#region CurrentLayoutChanged

		public event EventHandler<CurrentLayoutEventArgs> CurrentLayoutChanged;

		void OnCurrentLayoutChanged(StmModuleFilter previous, StmModuleFilter current)
		{
			if (CurrentLayoutChanged != null)
			{
				CurrentLayoutChanged(this, new CurrentLayoutEventArgs(previous, current));
			}
		}

		public class CurrentLayoutEventArgs : EventArgs
		{
			public CurrentLayoutEventArgs(StmModuleFilter previous, StmModuleFilter current)
			{
				Previous = previous;
				Current = current;
			}

			public readonly StmModuleFilter Previous;
			public readonly StmModuleFilter Current;
		}

		#endregion

		#endregion

		public const string SingleQuote = "'";
		public const string SingleQuoteEscaped = @"\'";

		#region Layouts

		public bool IsValidLayoutName(ZString layoutName)
		{
			return Layouts.ContainsCode(layoutName);
		}

		public CodeDescriptionPairList Layouts
		{
			get
			{
				var result = new CodeDescriptionPairList();

				result.AddPair("", "");

				foreach (var filter in Parent.Layouts_PublishedOnly)
				{
					result.AddPair(filter, filter.DisplayName, filter.DisplayName);
				}

				foreach (var filter in Parent.Layouts_UnpublishedOnly)
				{
					result.AddPair(filter, filter.DisplayName, filter.DisplayName);
				}

				return result;
			}
		}

		readonly FilterStripBusinessObject Parent;

		#endregion

		#region Current User

		protected override ZGuid GetCurrentUserPk()
		{
			if (LoggedInUser == null)
			{
				return ZGuid.Empty;
			}
			return LoggedInUser.PK;
		}

		protected override ZGuid GetCurrentOrganisationPk()
		{
			if (LoggedInUser == null)
			{
				return ZGuid.Empty;
			}
			return (LoggedInUser as OrgContact)?.OC_OH ?? ZGuid.Empty;
		}

		protected override ZString GetCurrentUserTablePrefix()
		{
			IBusiness business = LoggedInUser as IBusiness;
			return (business != null)
				? ObjectFactory.Get<IApplicationSchemaResolver>().GetColumnNamePrefix(business.TableName)
				: OrgContactSchema.Constants.Prefix;
		}

		readonly IContactable LoggedInUser;

		#endregion

		#region DeleteCurrentLayout

		public void DeleteCurrentLayout()
		{
			CurrentLayout = null;
			fIsCurrentLayoutDeleted = true;
			Parent.ResetLastUsedLayout();
		}

		public void ResetIsCurrentLayoutDeleted()
		{
			fIsCurrentLayoutDeleted = false;
		}

		public bool IsCurrentLayoutDeleted
		{
			get { return fIsCurrentLayoutDeleted; }
		}

		bool fIsCurrentLayoutDeleted;

		#endregion
	}
}
