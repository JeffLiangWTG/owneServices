using System;

namespace Enterprise.Services.OperationalActions.Integration
{
	public interface IOperationalActionFieldTester
	{
		void TestFields<T>(bool testChildren);
		void TestFields(Type type, bool testChildren);
		bool IsFieldUnsupported(Type type, string propertyName);
	}
}
