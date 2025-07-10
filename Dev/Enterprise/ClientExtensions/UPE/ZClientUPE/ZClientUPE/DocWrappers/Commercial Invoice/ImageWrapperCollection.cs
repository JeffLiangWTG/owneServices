using System;
using System.Drawing;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Client.UPE.Business
{
	public class ImageWrapperCollection : NonPersistentBusinessObjectCollection<ImageWrapper>, IBODocDataProviderCollection
	{
		public ImageWrapperCollection(BusinessObjectFactory factoryToWrap)
			: base(factoryToWrap)
		{
		}

		public void Add(Image image)
		{
			base.Add(new ImageWrapper(new Bitmap(image)));
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			throw new NotSupportedException("Document wrappers do not support creating brand new objects. They only wrap existing objects. Use Load(IEnumerable objectsToWrap) instead.");
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		#region IBODocDataProviderCollection Members

		IBODocDataProviderCollectionHelper Helper
		{
			get { return helper ?? (helper = (IBODocDataProviderCollectionHelper)Activator.CreateInstance(ObjectFactory.GetType<IBODocDataProviderCollectionHelper>(), new object[] { this })); }
		}
		IBODocDataProviderCollectionHelper helper;

		IBODocDataProvider IBODocDataProviderCollection.this[string index]
		{
			get { return null; }
		}

		IBODocDataProvider IBODocDataProviderCollection.this[int index]
		{
			get { return BODocDataProvider.Get(this[index]); }
		}

		int IBODocDataProviderCollection.Count
		{
			get { return Count; }
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
