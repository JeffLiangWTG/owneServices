using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.Integration.Freight;
using Enterprise.ZArchitecture.Schema;
using ICusEntryNumAdditionalReferenceCollection = Enterprise.Integration.Customs.ICusEntryNumAdditionalReferenceCollection;
using ICusEntryNumber = Enterprise.Integration.Customs.ICusEntryNumber;

namespace Enterprise.Customs.Common
{
	public sealed class CusEntryNumAdditionalReferenceCollection : DependentBusinessObjectCollection<CusEntryNumber, BusinessObject>, IBODocDataProviderCollection, ICusEntryNumAdditionalReferenceCollection
	{
		public CusEntryNumAdditionalReferenceCollection(BusinessObject parent)
			: base(parent)
		{
		}

		#region IBODocDataProviderCollection Members

		IBODocDataProviderCollectionHelper Helper
		{
			get { return helper ?? (helper = (IBODocDataProviderCollectionHelper)Activator.CreateInstance(ObjectFactory.GetType<IBODocDataProviderCollectionHelper>(), new object[] { this })); }
		}
		IBODocDataProviderCollectionHelper helper;

		IBODocDataProvider IBODocDataProviderCollection.this[string index]
		{
			get { return GetRow(index); }
		}

		IBODocDataProvider IBODocDataProviderCollection.this[int index]
		{
			get { return Helper[index]; }
		}

		int IBODocDataProviderCollection.Count
		{
			get { return Helper.Count; }
		}

		ZString IBODocDataProviderCollection.Format(ZString formatString, ZString delimiter, ZString filterString, ZString groupByParameters, ZInt maxItems)
		{
			return Helper.Format(formatString, delimiter, filterString, groupByParameters, maxItems);
		}

		object IBODocDataProviderCollection.Total(ZString fieldName, ZString decimalPlaces, ZString filter)
		{
			return Helper.Total(fieldName, decimalPlaces, filter);
		}

		IBODocDataProvider GetRow(ZString index)
		{
			var criteria = new CusEntryNumSearchCriteria(index);
			var entryNumber = (CusEntryNumber)criteria.Find(this.Cast<ISearcheableCusEntryNumber>());

			if (entryNumber != null)
			{
				return BODocDataProvider.Get(entryNumber);
			}

			return Helper[index];
		}

		public BusinessObject Find(ZString match)
		{
			return Helper.Find(match);
		}

		#endregion

		#region Default Values

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			var num = (CusEntryNumber)child;

			num.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;
			num.Parent = Master;
			num.CE_EntryIsSystemGenerated = false;
		}

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			((CusEntryNumber)bizOAdded).Parent = Master;
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			var packLineSynchroniseProvider = Master as IPackLineSynchroniseProvider;
			if (packLineSynchroniseProvider != null && packLineSynchroniseProvider.PackLineSynchronise != null)
			{
				packLineSynchroniseProvider.PackLineSynchronise.MarkSyncDirty();
			}
		}

		#endregion

		#region Filter

		protected override SchemaGuidColumn FKSchemaColumnInDependent
		{
			get { return CusEntryNumSchema.CE_ParentID; }
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZQuery query = base.CreateRelationshipFilter();
			query.AddToFilter(CusEntryNumSchema.CE_Category, CusEntryNumber.Categories.AdditionalReferenceNumber);
			return query;
		}

		#endregion

		#region New Methods

		public CusEntryNumber AddNewIfNotExist(ZString type, ZString number)
		{
			CusEntryNumber result = null;
			var numberTrimExtraChar = number.SubstringSafe(0, CusEntryNumSchema.CE_EntryNum.MaxLength);
			if (!type.IsEmpty && !number.IsEmpty && !this.Cast<CusEntryNumber>().Any(n => n.CE_EntryType == type && n.CE_EntryNum == numberTrimExtraChar))
			{
				result = AddNew();
				result.CE_EntryType = type;
				result.CE_EntryNum = numberTrimExtraChar;
			}

			return result;
		}

		public ZString[] GetAllReferenceNumbersByType(ZString type)
		{
			return
				this.Cast<CusEntryNumber>()
				.Where(number => number.CE_EntryType == type)
				.Select(number => number.CE_EntryNum)
				.ToArray();
		}

		public ZString[] GetAllReferenceNumbersByTypeAndCountry(ZString type, ZString countryCode)
		{
			return
				this.Cast<CusEntryNumber>()
				.Where(number => number.CE_EntryType == type && number.CE_RN_NKCountryCode == countryCode)
				.Select(number => number.CE_EntryNum)
				.ToArray();
		}

		public CusEntryNumber GetFirstReferenceNumberByType(ZString type)
		{
			foreach (CusEntryNumber number in this)
			{
				if (number.CE_EntryType == type)
				{
					return number;
				}
			}
			return null;
		}

		public CusEntryNumber GetFirstReferenceNumberByTypeAndCountry(ZString type, ZString countryCode)
		{
			foreach (CusEntryNumber number in this)
			{
				if (number.CE_EntryType == type && number.CE_RN_NKCountryCode == countryCode)
				{
					return number;
				}
			}
			return null;
		}

		#endregion

		#region Parentless CusEntryNumber issue see WI00031939

		public override void Remove(BusinessObject businessObject)
		{
			CusEntryNumber cusEntryNumber = businessObject as CusEntryNumber;

			if (!SuspendDeveloperNotification && cusEntryNumber != null && !cusEntryNumber.IsDeleted && !cusEntryNumber.IsDeleting && cusEntryNumber.IsInDatabase)
			{
				string message = string.Format(
"CusEntryNumber PK:{0}, CE_EntryType:{1}, CE_EntryNum:{2} was removed from CusEntryNumAdditionalReferenceCollection but not deleted. " +
"The collection: AllowRemove:{3}, ReadOnly:{4}",
					cusEntryNumber.PK,
					cusEntryNumber.CE_EntryType,
					cusEntryNumber.CE_EntryNum,
					AllowRemove,
					ReadOnly);

				CargoWise.Common.ErrorReporter.ReportOnce("MEP:CusEntryNumAdditionalReferenceCollection:Remove", message);
			}

			base.Remove(businessObject);
		}

		public override void RemoveAndDelete(BusinessObject elementToDelete)
		{
			using (new RemoveErrorReporterSuspender(this))
			{
				base.RemoveAndDelete(elementToDelete);
			}
		}

		class RemoveErrorReporterSuspender : IDisposable
		{
			public RemoveErrorReporterSuspender(CusEntryNumAdditionalReferenceCollection collection)
			{
				this.collection = collection;
				this.collection.SuspendDeveloperNotification = true;
			}

			readonly CusEntryNumAdditionalReferenceCollection collection;

			void IDisposable.Dispose()
			{
				collection.SuspendDeveloperNotification = false;
			}
		}

		bool SuspendDeveloperNotification { get; set; }

		#endregion

		#region ICusEntryNumAdditionalReferenceCollection Members

		ICusEntryNumber ICusEntryNumAdditionalReferenceCollection.AddNew()
		{
			return AddNew();
		}

		ICusEntryNumber ICusEntryNumAdditionalReferenceCollection.AddNewIfNotExist(ZString type, ZString number)
		{
			return AddNewIfNotExist(type, number);
		}

		ICusEntryNumber ICusEntryNumAdditionalReferenceCollection.GetFirstReferenceNumberByType(ZString type)
		{
			return GetFirstReferenceNumberByType(type);
		}

		ICusEntryNumber ICusEntryNumAdditionalReferenceCollection.this[int i]
		{
			get { return this[i]; }
		}

		void ICusEntryNumAdditionalReferenceCollection.RemoveAndDelete(ICusEntryNumber entryNum)
		{
			this.RemoveAndDelete((BusinessObject)entryNum);
		}

		#endregion

		public ZString AllNumbersAsString => string.Join(", ", this.Cast<CusEntryNumber>().Select(num => FormatEntryAsTypeNumCountry(num)));

		string FormatEntryAsTypeNumCountry(CusEntryNumber cusEntryNum)
		{
			var typeSeparator = cusEntryNum.CE_EntryNum.IsEmpty && cusEntryNum.CE_RN_NKCountryCode.IsEmpty
				? string.Empty
				: ": ";

			var numberSeparator = cusEntryNum.CE_RN_NKCountryCode.IsEmpty
				? string.Empty
				: "/";

			return cusEntryNum.CE_EntryType + typeSeparator + cusEntryNum.CE_EntryNum + numberSeparator + cusEntryNum.CE_RN_NKCountryCode;
		}
	}

	#region CusEntryNumAdditionalReferenceCollectionProvider

	public class CusEntryNumAdditionalReferenceCollectionProvider : Integration.Customs.ICusEntryNumAdditionalReferenceCollectionProvider
	{
		public ICusEntryNumAdditionalReferenceCollection GetCollection(BusinessObject parent)
		{
			return new CusEntryNumAdditionalReferenceCollection(parent);
		}
	}

	#endregion
}
