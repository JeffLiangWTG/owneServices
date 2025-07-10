using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Schema;

namespace Enterprise.Security.ActiveDirectory
{
	public class AttributeMapItemCollection : NonPersistentBusinessObjectCollection<AttributeMapItem>
	{
		public AttributeMapItemCollection()
			: base()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AttributeMapItem();
		}

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override bool AllowRemoveCore
		{
			get { return false; }
		}

		public AttributeMapItem this[SchemaColumn schemaColumn]
		{
			get { return this.Cast<AttributeMapItem>().FirstOrDefault(item => item.Matches(schemaColumn)); }
		}

		protected override bool RunPreSaveValidationCore()
		{
			var result = base.RunPreSaveValidationCore();

			ValidateForDuplicatedMapping();
			return result;
		}

		void ValidateForDuplicatedMapping()
		{
			var duplication = this.Cast<AttributeMapItem>().GroupBy(a => new { a.ActiveDirectoryAttributeName, a.EnterpriseTableName }).Where(g => g.Count() > 1);
			foreach (var item in duplication)
			{
				for (int i = 0; i < item.Count(); i++)
				{
					item.ElementAt(i).ActiveDirectoryAttributeNameInfo.AddError(Res.GetString("F8CD888B-FDCF-4528-BD2A-26569B5CC4FF", "The '{0}' attribute has been mapped to more than one column in {1} table.", item.Key.ActiveDirectoryAttributeName, item.Key.EnterpriseTableName));
				}
			}
		}
	}
}
