using System;
using CargoWise.Data;
using CargoWise.EntityFramework;

namespace Enterprise.WorkflowManager.ServiceTasks.Testing
{
	sealed class TestBusinessObjectFactoryForExceptionThrowing : BusinessObjectFactory
	{
		#region Properties

		public bool ThrowExceptionOnSave { get; set; }
		public int TimesBeforeExceptionIsThrown { get; set; }

		int counter;

		bool ThrowException
		{
			get { return ThrowExceptionOnSave && counter == TimesBeforeExceptionIsThrown && ExceptionToThrow != null; }
		}

		public Exception ExceptionToThrow { get; set; }
		public Func<BusinessObjectFactory> CreateNewFactoryMethod { get; set; }

		#endregion

		#region Constructors

		public TestBusinessObjectFactoryForExceptionThrowing()
			: base()
		{
		}

		public TestBusinessObjectFactoryForExceptionThrowing(string databaseName)
			: base(databaseName)
		{
		}

		public TestBusinessObjectFactoryForExceptionThrowing(DbConnection connection)
			: base(connection)
		{
		}

		#endregion

		#region Overrides

		protected override void SaveCore()
		{
			if (ThrowException)
			{
				counter = 0;
				throw ExceptionToThrow;
			}
			else if (TimesBeforeExceptionIsThrown > 0)
			{
				counter++;
			}

			base.SaveCore();
		}

		public override BusinessObjectFactory CreateNewFactory(bool usingMyThreadSentry = false)
		{
			if (CreateNewFactoryMethod != null)
			{
				return CreateNewFactoryMethod();
			}

			return new BusinessObjectFactory();
		}

		#endregion
	}
}
