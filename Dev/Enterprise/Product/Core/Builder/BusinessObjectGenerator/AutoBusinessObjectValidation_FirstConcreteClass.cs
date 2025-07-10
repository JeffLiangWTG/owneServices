
namespace Enterprise.BusinessObjectGenerator
{
	public class AutoBusinessObjectValidation_FirstConcreteClass : AutoSourceFile
	{
		public AutoBusinessObjectValidation_FirstConcreteClass(BusinessObjectInfo info)
		{
			this.Info = info;
		}

		#region Code for Heading Comment

		protected override string HeadingComment
		{
			get
			{
				return LinesOfCode(
					"//--------------------------------------------------------------------------------------------------",
					"// <important>",
					"//",
					"//    DO NOT CHANGE THE NAME OF THIS CLASS OR THE CONSTRUCTOR'S SIGNATURE",
					"//    THIS CLASS SHOULD ALWAYS INHERIT FROM " + Info.ClassNames.BusinessObjectAuto + "Validation",
					"//",
					"//    This class should be used for overriding validation in " + Info.ClassNames.BusinessObjectAuto + "Validation.",
					"//",
					"// </important>",
					"//--------------------------------------------------------------------------------------------------",
					"");
			}
		}

		#endregion

		#region Code for Source Body

		protected override string Body
		{
			get
			{
				if (fBody == null)
				{
					fBody = LinesOfCode(
						"namespace " + Namespace,
						"{",
						"	using CargoWise.ComponentModel;",
						"	using Enterprise.ZArchitecture.Business;",
						"",
						"	public class " + ClassName + " : " + InheritsFrom,
						"	{",
								CodeForConstructor.TrimEnd(),
						"	}",
						"}",
						"",
						CodeForTest
						);
				}

				return fBody;
			}
		}
		string fBody;

		#endregion

		#region Code for Constructor

		protected string CodeForConstructor
		{
			get
			{
				return LinesOfCode(
					"		public " + ClassName + "(" + Info.ClassNames.BusinessObjectAuto + " parent) : base(parent)",
					"		{",
					"		}",
					"");
			}
		}

		#endregion

		#region Code for Test

		protected string CodeForTest
		{
			get
			{
				return LinesOfCode(
					"#region Test",
					"#if DEBUG",
					"",
					"namespace " + Namespace + ".Testing",
					"{",
					"	using CargoWise.EntityFramework.Testing;",
					"	using NUnit.Framework;",
					"",
					"	internal class " + ClassName + "Test : BusinessObjectValidationTestCase",
					"	{",
					"	}",
					"}",
					"",
					"#endif",
					"#endregion",
					"");
			}
		}

		#endregion

		protected string Namespace
		{
			get { return Info.Namespace; }
		}

		protected string ClassName
		{
			get { return Info.ClassNames.Validation; }
		}

		protected string InheritsFrom
		{
			get { return Info.ClassNames.ValidationAuto; }
		}

		readonly BusinessObjectInfo Info;
	}
}