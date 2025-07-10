using System;
using System.Collections;
using System.Linq.Expressions;
using System.Text;
using CargoWise.Customs.JP.MessageContracts;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngineCore.DocWrappers;

namespace Enterprise.Customs.JP.Common.Testing;

abstract class InboundMessageDocumentItemWrapperTest<TItem, TParent> : NonPersistentBusinessObjectTestCase
	where TItem : DocumentWrapper
	where TParent : DocumentWrapper
{
	protected abstract string GetDefaultMessageTestFile();

	protected TParent ParentWrapper => parentWrapper ??= (TParent)Activator.CreateInstance(typeof(TParent), GetInboundMessageParseResult(), Factory);
	TParent parentWrapper;

	IJPInboundMessageParseResult GetInboundMessageParseResult()
	{
		var parser = NACCSFactoryService.GetInboundMessageParser(Factory);
		return parser.Parse(Encoding.ASCII.GetBytes(TestDataHelper.GetResourceStream(GetDefaultMessageTestFile())));
	}

	protected TItem DocItemWrapper => docItemWrapper ??= CreateItemWrapper(ParentWrapper);
	TItem docItemWrapper;

	protected virtual TItem CreateItemWrapper(TParent parentWrapper) => CreateItemsDynamicFunc().Invoke(parentWrapper) is IList list && list.Count > 0 ? list[0] as TItem : default;

	static Func<TParent, IList> CreateItemsDynamicFunc()
	{
		var parentType = typeof(TParent);
		var itemsProperty = parentType.GetProperty("Items");
		var lambdaParameter = Expression.Parameter(parentType);
		var itemsLambdaExpression = Expression.Lambda(Expression.Property(lambdaParameter, itemsProperty.Name), lambdaParameter);
		return (Func<TParent, IList>)itemsLambdaExpression.Compile();
	}

	protected override BusinessObject GetNewBusinessObject() => DocItemWrapper;
}
