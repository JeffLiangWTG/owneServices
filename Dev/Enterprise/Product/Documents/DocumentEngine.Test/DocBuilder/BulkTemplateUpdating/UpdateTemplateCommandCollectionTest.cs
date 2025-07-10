using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.Testing
{
	[TestedType(typeof(UpdateTemplateCommandCollection))]
	sealed class UpdateTemplateCommandCollectionTest : NonPersistentBusinessObjectCollectionTestCase<UpdateTemplateCommandCollection>
	{
		protected override UpdateTemplateCommandCollection GetCollectionToTest()
		{
			return new UpdateTemplateCommandCollection(Factory);
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			return new DummyUpdateTemplateCommand(Factory);
		}

		#region Implementation

		class DummyUpdateTemplateCommand : UpdateTemplateCommand
		{
			internal DummyUpdateTemplateCommand(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			public override ZString Description
			{
				get { return "DummyUpdateTemplateCommand"; }
			}

			public override void Execute()
			{
			}
		}

		#endregion
	}
}
