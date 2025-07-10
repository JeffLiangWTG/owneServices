using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Business.Internal;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.ComponentModel;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business
{
	[ModuleID(ModuleId.AlternateGLAccounts)]
	public class AlternateGLAccountCombineParentAccountCollection : NonPersistentBusinessObjectCollection<AlternateGLAccountCombineParentAccount>
	{
		public AlternateGLAccountCombineParentAccountCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		#region load

		public void SetFilterHelper(AlternateGLAccountFilterHelper filterHelper)
		{
			this.FilterHelper = filterHelper;
		}

		AlternateGLAccountFilterHelper FilterHelper;

		public override void Load(ZQuery unusedFilter)
		{
			Load();
		}

		public override void Load()
		{
			if (FilterHelper != null)
			{
				LoadUsingFilterHelper(FilterHelper);
			}
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		public override int GetEstimatedLoadCount(ZQuery completeFilter)
		{
			ZInt countToReturn = 0;
			if (FilterHelper != null)
			{
				var sQL = string.Format("SELECT COUNT(*) AS Count FROM ({0}) m", FilterHelper.PlainFilterWithoutParamValues);
				var countCollection = new DynamicBusinessObjectCollection(Factory);
				countCollection.Load(sQL, FilterHelper.FilterParameters);
				countToReturn = (ZInt)countCollection[0]["Count"];
			}

			return countToReturn;
		}

		void LoadUsingFilterHelper(AlternateGLAccountFilterHelper filterHelper)
		{
			using (SuspendListChanged())
			{
				RemoveAll();
				LoadFilterHelper(Factory, filterHelper);
			}

			if (AfterLoaded != null)
			{
				AfterLoaded(this, new EventArgs());
			}
		}

		public event EventHandler AfterLoaded;

		#endregion

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AlternateGLAccountCombineParentAccount(Factory);
		}

		public void LoadFilterHelper(BusinessObjectFactory factory, AlternateGLAccountFilterHelper filterHelper)
		{
			var dynBizOs = new DynamicBusinessObjectCollection(factory);
			dynBizOs.Load(filterHelper.PlainFilterWithoutParamValues, filterHelper.FilterParameters);
			var alternateGLAccountPKs = dynBizOs.Select(bizo => bizo[AccAlternateGLAccount.Schema.PK]).ToList();
			var dBOnlyQuery = new ZDBOnlyQuery(typeof(AccAlternateGLAccount));
			dBOnlyQuery.AddToFilter(AccAlternateGLAccountSchema.PK, alternateGLAccountPKs);
			var alternateGLAccountDictionary = factory.Load<AccAlternateGLAccount>(dBOnlyQuery).ToDictionary(alternateGLAccount => alternateGLAccount.PK);
			var query = new ZQuery();
			query.AddToFilter(OrgHeaderSchema.PK, dynBizOs.Select(bizo => new ZGuid((ZString)bizo[AlternateGLAccountCombineParentAccount.Schema.Attribute_ORG])).Distinct());
			var orgDictionary = factory.Load<OrgHeader>(query).ToDictionary(x => x.PK, y => y);

			dynBizOs.ForEach(bizo =>
			{
				var alternateGLAccount = alternateGLAccountDictionary.GetValueSafe((ZGuid)bizo[AccAlternateGLAccount.Schema.PK]);
				var conbine = this.AddNew();
				conbine.AlternateGLAccount = alternateGLAccount;
				conbine.GLHeaderPK = (ZGuid)bizo[AccAlternateGLAccountAttribute.Schema.AAA_AG_GLHeader];
				orgDictionary.TryGetValue(new ZGuid((ZString)bizo[AlternateGLAccountCombineParentAccount.Schema.Attribute_ORG]), out var org);
				conbine.ORGAttribute = org?.OH_Code ?? string.Empty;
				conbine.OCG_Attribute = (ZString)bizo[AlternateGLAccountCombineParentAccount.Schema.Attribute_OCG];
				conbine.SPR_Attribute = (ZString)bizo[AlternateGLAccountCombineParentAccount.Schema.Attribute_SPR];
				conbine.TIC_Attribute = (ZString)bizo[AlternateGLAccountCombineParentAccount.Schema.Attribute_TIC];
				conbine.LFO_Attribute = (ZString)bizo[AlternateGLAccountCombineParentAccount.Schema.Attribute_LFO];
				conbine.LFE_Attribute = (ZString)bizo[AlternateGLAccountCombineParentAccount.Schema.Attribute_LFE];
			});
		}

		protected override IFindBoxListProvider FindBoxListProvider
		{
			get
			{
				return new AlternateGLAccountCombineParentAccountCollectionFindBoxListProvider(this);
			}
		}
	}

	class AlternateGLAccountCombineParentAccountCollectionFindBoxListProvider : NonPersistentBusinessObjectFindBoxListProvider
	{
		public AlternateGLAccountCombineParentAccountCollectionFindBoxListProvider(AlternateGLAccountCombineParentAccountCollection collection)
			: base(collection)
		{
		}

		public override string CodeFromPrimaryKey(ZGuid pK)
		{
			var account = List.Factory.LoadTop1<AccAlternateGLAccount>(new ZQuery(AccAlternateGLAccountSchema.PK, pK));
			return account?.AGA_AccountNum ?? string.Empty;
		}

		public override string DescriptionFromPrimaryKey(ZGuid pK)
		{
			var account = List.Factory.LoadTop1<AccAlternateGLAccount>(new ZQuery(AccAlternateGLAccountSchema.PK, pK));
			return account?.AGA_Description ?? string.Empty;
		}

		public override ZGuid PrimaryKeyFromCode(string code)
		{
			if (!string.IsNullOrEmpty(code))
			{
				var query = new ZDBOnlyQuery(typeof(AccAlternateGLAccount));
				query.AddToFilter(new ZQuery(AccAlternateGLAccountSchema.AGA_AccountNum, code));
				return List.Factory.LoadTop1<AccAlternateGLAccount>(query)?.PK ?? ZGuid.Empty;
			}
			return ZGuid.Empty;
		}
	}
}
