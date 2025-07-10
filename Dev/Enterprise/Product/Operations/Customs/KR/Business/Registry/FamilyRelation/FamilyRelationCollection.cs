using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.KR.Business
{
	[XmlSerializerAssembly("Enterprise.Customs.KR.Business.XmlSerializers")]
	public sealed class FamilyRelationCollection : RegistryBusinessObjectCollectionTemplate, ICodeDescriptionPairList
	{
		public FamilyRelationCollection() : base() { }

		public FamilyRelationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory) { }

		public new FamilyRelation this[int i]
		{
			get { return (FamilyRelation)Elements[i]; }
		}

		public new FamilyRelation AddNew()
		{
			return (FamilyRelation)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new FamilyRelation(CurrentFallbackLevel, CurrentFactory, this);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new FamilyRelationCollection(fallbackLevel, factory);
		}

		public static FamilyRelationCollection GetDefaultFamilyRelationCollection()
		{
			return new FamilyRelationCollection()
			{
				new FamilyRelation() { Code = RelationshipCodeList.Codes._00, Description = RelationshipCodeList.Descriptions._00, IsSystemOne = true },
				new FamilyRelation() { Code = RelationshipCodeList.Codes._01, Description = RelationshipCodeList.Descriptions._01, IsSystemOne = true },
				new FamilyRelation() { Code = RelationshipCodeList.Codes._02, Description = RelationshipCodeList.Descriptions._02, IsSystemOne = true },
				new FamilyRelation() { Code = RelationshipCodeList.Codes._03, Description = RelationshipCodeList.Descriptions._03, IsSystemOne = true },
				new FamilyRelation() { Code = RelationshipCodeList.Codes._04, Description = RelationshipCodeList.Descriptions._04, IsSystemOne = true },
				new FamilyRelation() { Code = RelationshipCodeList.Codes._05, Description = RelationshipCodeList.Descriptions._05, IsSystemOne = true },
				new FamilyRelation() { Code = RelationshipCodeList.Codes._06, Description = RelationshipCodeList.Descriptions._06, IsSystemOne = true },
				new FamilyRelation() { Code = RelationshipCodeList.Codes._07, Description = RelationshipCodeList.Descriptions._07, IsSystemOne = true },
				new FamilyRelation() { Code = RelationshipCodeList.Codes._08, Description = RelationshipCodeList.Descriptions._08, IsSystemOne = true },
				new FamilyRelation() { Code = RelationshipCodeList.Codes._09, Description = RelationshipCodeList.Descriptions._09, IsSystemOne = true },
				new FamilyRelation() { Code = RelationshipCodeList.Codes._10, Description = RelationshipCodeList.Descriptions._10, IsSystemOne = true }
			};
		}

		bool ICodeDescriptionPairList.ContainsCode(object code)
		{
			return this.Cast<FamilyRelation>().Any(x => x.Code == code.ToString());
		}

		string ICodeDescriptionPairList.GetDescriptionFromCode(string code)
		{
			return this.Cast<FamilyRelation>().FirstOrDefault(x => x.Code.ToString() == code)?.Description ?? ZString.Empty;
		}
	}
}
