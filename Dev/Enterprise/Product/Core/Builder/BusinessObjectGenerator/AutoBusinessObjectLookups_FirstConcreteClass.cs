
namespace Enterprise.BusinessObjectGenerator
{
	public class AutoBusinessObjectLookups_FirstConcreteClass : AutoSourceFile
	{
		public AutoBusinessObjectLookups_FirstConcreteClass(BusinessObjectInfo info)
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
					"//    THIS CLASS SHOULD ALWAYS INHERIT FROM " + Info.ClassNames.BusinessObjectAuto + "Lookups",
					"//",
					"//    This class should be used for overriding collections in " + Info.ClassNames.BusinessObjectAuto + "Lookups",
					"//    (for example to add filtering), or for adding your own lookup collections.",
					"//",
					"//    ALL FINDBOXES SHOULD BIND TO THESE COLLECTIONS (and you will get automatic list validation!)",
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

						UsingClause,
						"",
						"namespace " + Namespace,
						"{",
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

		#region Code for Using Clause

		protected string UsingClause
		{
			get
			{
				return LinesOfCode(
					"using CargoWise.ComponentModel;",
					"using Enterprise.MasterFiles.Business;",
					"using CargoWise.Types;",
					"using Enterprise.ZArchitecture;");
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
					"	internal class " + ClassName + "Test : BusinessObjectLookupsTestCase",
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
			get { return Info.ClassNames.Lookups; }
		}

		protected string InheritsFrom
		{
			get { return Info.ClassNames.LookupsAuto; }
		}

		readonly BusinessObjectInfo Info;
	}
}