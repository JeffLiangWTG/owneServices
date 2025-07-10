using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.KR.Business
{
	public class EDIMessageWrapperCollection : NonPersistentBusinessObjectCollection<EDIMessageWrapper>, IBODocDataProviderCollection
	{
		public EDIMessageWrapperCollection(IEnumerable<EDIMessage> messages, BusinessObjectFactory factory) : base(factory)
		{
			if (ReferenceEquals(messages, null))
			{
				throw new ArgumentNullException(nameof(messages), "Messages");
			}

			this.messages = messages;
			PopulateElements();
		}

		readonly IEnumerable<EDIMessage> messages;

		void PopulateElements()
		{
			foreach (EDIMessage message in messages)
			{
				Add(new EDIMessageWrapper(message));
			}
		}

		protected override bool AllowNewCore => false;
		protected override BusinessObject CreateNonPersistentBusinessObject() => throw new NotSupportedException();

		#region IBODocDataProviderCollection

		IBODocDataProviderCollectionHelper Helper
		{
			get { return helper ?? (helper = (IBODocDataProviderCollectionHelper)Activator.CreateInstance(ObjectFactory.GetType<IBODocDataProviderCollectionHelper>(), new object[] { this })); }
		}
		IBODocDataProviderCollectionHelper helper;

		IBODocDataProvider IBODocDataProviderCollection.this[string index]
		{
			get { return Helper[index]; }
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

		public BusinessObject Find(ZString match)
		{
			return Helper.Find(match);
		}

		#endregion
	}
}
