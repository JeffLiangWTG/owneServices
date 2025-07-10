
using Enterprise.BusinessObjectGenerator;

namespace Enterprise.Builder.Generator
{
	/// <summary>
	/// This generator is used for business objects that live in ZArchitecture.sln
	/// </summary>
	public class SingleBizObjLivesInZArchitectureGenerator : SingleBizObjGenerator
	{
		public SingleBizObjLivesInZArchitectureGenerator(string fileNameOfBusinessObject, GeneratorOutputDirectory outputDirectory)
			: base(fileNameOfBusinessObject, outputDirectory)
		{
		}

		public override string[] ListOfFilesToBeGenerated
		{
			get
			{
				return new string[]
				{
					FileNameOfBusinessObject,
					FileNameOfBusinessObjectSchema,
					FileNameOfBusinessObjectValidation,
					FileNameOfBusinessObjectValidationConcreteClass,
					"No Auto Lookups file is generated for a business object that lives in ZArchitecture.sln.",
					"No Concrete Lookups file is generated for a business object that lives in ZArchitecture.sln."
				};
			}
		}

		protected override bool IsRegenRequiredForLookups()
		{
			return false; // no lookups for a bizobj that lives in ZArchitecture (circular reference issue)
		}

		protected override bool IsRegenRequiredForLookupsConcreteClass()
		{
			return false; // no lookups for a bizobj that lives in ZArchitecture (circular reference issue)
		}

		#region CodeCollection

		protected internal override AutoBusinessObjectCodeCollection CodeCollection
		{
			get
			{
				if (fCodeCollection == null)
				{
					bool isInZArchitectureSolution = true;
					fCodeCollection = new AutoBusinessObjectCodeCollection(Info, isInZArchitectureSolution);
				}
				return fCodeCollection;
			}
		}
		AutoBusinessObjectCodeCollection fCodeCollection;

		#endregion
	}
}
