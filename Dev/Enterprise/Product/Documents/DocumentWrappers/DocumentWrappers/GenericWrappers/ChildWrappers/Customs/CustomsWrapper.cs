using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngineCore.DocWrappers;
using static Enterprise.Integration.Customs.CA;

namespace Enterprise.DocumentWrappers.GenericWrappers.ChildWrappers.Customs
{
	public class CustomsWrapper : DocumentWrapper
	{
		public static CustomsWrapper New(ICustomsInfo customsInfo, BusinessObjectFactory factory)
		{
			return new CustomsWrapper(customsInfo, factory);
		}

		CustomsWrapper(ICustomsInfo customsInfo, BusinessObjectFactory factory)
			: base(customsInfo, factory)
		{
			this.customsInfo = customsInfo;
		}

		readonly ICustomsInfo customsInfo;

		ICACustomsWrapper canadaWrapper;
		public ICACustomsWrapper CA
		{
			get
			{
				if (canadaWrapper == null)
				{
					var declaration = customsInfo.Declaration;
					if (declaration != null)
					{
						var types = ObjectFactory.GetType<ICACustomsWrapper>();
						canadaWrapper = (ICACustomsWrapper)Activator.CreateInstance(types, declaration, Factory);
					}
				}

				return canadaWrapper;
			}
		}
	}
}
