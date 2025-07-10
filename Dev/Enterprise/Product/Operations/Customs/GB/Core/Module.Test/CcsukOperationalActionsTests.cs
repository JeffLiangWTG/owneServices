using Enterprise.Customs.GB.Module.OperationalActions.DropOff;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Module.OperationalActions.Ccsuk.Testing
{
	[TestedType(typeof(DetachFromForwardingMethod))]
	class DetachFromForwardingMethodMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<DetachFromForwardingMethod>
	{
		protected override DetachFromForwardingMethod NewMethod()
		{
			return new DetachFromForwardingMethod();
		}
	}

	[TestedType(typeof(RenominateMethod))]
	class RenominateMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<RenominateMethod>
	{
		protected override RenominateMethod NewMethod()
		{
			return new RenominateMethod();
		}
	}

	[TestedType(typeof(QueryWithUpdateMethod))]
	class QueryWithUpdateMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<QueryWithUpdateMethod>
	{
		protected override QueryWithUpdateMethod NewMethod()
		{
			return new QueryWithUpdateMethod();
		}
	}

	[TestedType(typeof(CcsukOperationalActionMethodProvider))]
	class CcsukOperationalActionMethodProviderTest : Services.OperationalActions.Support.Testing.OperationalActionMethodProviderTest
	{
		protected override Services.OperationalActions.Support.ActionMethodProviderID ID
		{
			get { return Enterprise.Services.OperationalActions.Support.ActionMethodProviderIDs.GbCcsuk; }
		}
	}

	[TestedType(typeof(GbDeclarationDropOffMessageOperationalActionMethod))]
	class GbDeclarationDropOffMessageOperationalActionMethodTest : Services.OperationalActions.Support.Testing.OperationalActionMethodTest<GbDeclarationDropOffMessageOperationalActionMethod>
	{
		protected override GbDeclarationDropOffMessageOperationalActionMethod NewMethod()
		{
			return new GbDeclarationDropOffMessageOperationalActionMethod();
		}
	}

	[TestedType(typeof(GbDeclarationDropOffMessageOperationalActionMethodProvider))]
	class GbDeclarationDropOffMessageOperationalActionMethodProviderTest : Services.OperationalActions.Support.Testing.OperationalActionMethodProviderTest
	{
		protected override Services.OperationalActions.Support.ActionMethodProviderID ID
		{
			get { return Enterprise.Services.OperationalActions.Support.ActionMethodProviderIDs.GbPickupDropOff; }
		}
	}
}
