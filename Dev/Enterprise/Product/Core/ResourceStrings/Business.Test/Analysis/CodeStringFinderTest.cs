using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Net.Http;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using NUnit.Framework;

namespace Enterprise.ResourceStrings.Business.Analysis.Testing
{
	[SuppressMessage("CargoWiseOne", "CW1199:Do Not Use Unnecessary Resource String In Unit Tests", Justification = "WI: WI00900276. These unit tests are for testing translations so changing to hardcoded strings is not appropriate.")]
	sealed class CodeStringFinderTest : TestCase
	{
		public void TestFindStringUsages()
		{
			var finder = new CodeStringFinder(this.GetType().Assembly.GetName().Name);
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "NoResStringProperty"));
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "ResStringProperty"), "T=Test");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "UnderScoreResStringProperty"), "T=UnderScore_Test");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "ResStringWithParamters"), "T=String {0} with paramter {1}");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "ResStringWithIfLocalVariable"), "T0=Zero", "T1=One", "T2=Two", "T=Other", "N=Negative");
			AssertResults(finder.FindStringUsages(typeof(TestClass2).FullName, "ResStringWithIfLocalVariable"), "T0=Zero", "T1=One", "T2=Two", "T=Other", "N=Negative");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "ResStringWithSwitch"), "AU=Australia", "US=United States", "UK=United Kingdom");
			AssertResults(finder.FindStringUsages(typeof(TestClass2).FullName, "ResStringWithSwitch"), "AU=Australia", "US=United States", "UK=United Kingdom");
			AssertResults(finder.FindStringUsages(typeof(TestClass2).FullName, "SelfRecursiveFunction"), "T=Test {0}");
			AssertResults(finder.FindStringUsages(typeof(TestClass2).FullName, "OutParameter"));
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "ResStringEmbedding"), "A=The number is {0}", "P=Positive", "N=Negative");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "ResStringParameter"), "A=The number is {0}");
			AssertResults(finder.FindStringUsages(typeof(TestClass2).FullName, "ResStringFromRegistry"), "FF2EDDB6-E751-4131-8784-D662E5877B38=Yours Sincerely,");
			AssertResults(finder.FindStringUsages(typeof(TestClass.InnerClass).FullName, "ResString"), "I=Inner");
			AssertResults(finder.FindStringUsages(typeof(TestClass2).FullName, "ResStringFromOverride"), "O=Override", "O3=Override from nested class");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "MultilingualString"), "M=Multilingual");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "UnderScoreMultilingualString"), "M=UnderScore_Multilingual");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "GetDataString"), "X=");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "GetCaculatedKeyString"), "=Test");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "SalesModeList"), "Commmon|SalesMode|Import=Import", "Commmon|SalesMode|Export=Export");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "DayOfWeekCodeList"), "AutoDayOfWeekCodeList|Sunday=Sunday", "AutoDayOfWeekCodeList|Monday=Monday", "AutoDayOfWeekCodeList|Tuesday=Tuesday", "AutoDayOfWeekCodeList|Wednesday=Wednesday", "AutoDayOfWeekCodeList|Thursday=Thursday", "AutoDayOfWeekCodeList|Friday=Friday", "AutoDayOfWeekCodeList|Saturday=Saturday");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "FactoryCachedValue"), "AutoDayOfWeekCodeList|Sunday=Sunday", "AutoDayOfWeekCodeList|Monday=Monday", "AutoDayOfWeekCodeList|Tuesday=Tuesday", "AutoDayOfWeekCodeList|Wednesday=Wednesday", "AutoDayOfWeekCodeList|Thursday=Thursday", "AutoDayOfWeekCodeList|Friday=Friday", "AutoDayOfWeekCodeList|Saturday=Saturday");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "CachedCodeDescriptionPairList"), "Commmon|SalesMode|Import=Import", "Commmon|SalesMode|Export=Export");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "DescriptionFromListWithFunctionInConstructor"), "A=Apple", "B=Banana");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "ReturnsResourceString"), "T=Test");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "VirtualResString2"), "O2=Override after skipped override");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "DescriptionFromDerivedList"), "A=Apple", "B=Banana");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "FactoryCachedValueWithDelegate"), "A=Alpha", "B=Beta");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "AnotherFactoryCachedValueWithDelegate"), "1=one", "2=two");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "DelegateWithState"), "3=three", "4=four");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "NeedHelp"), "H=Helped");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "StringFromWrapper"), "W=Wrapped");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "StringFromStaticConstructor"), "S=Static");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "TranslatableField"), "RS_Description$RGlyZWN0=Direct", "RS_Description$RG9vciB0byBEb29y=Door to Door", "RS_Description$RGVmZXJyZWQ==Deferred", "RS_Description$U3RhbmRhcmQ==Standard", "RS_Description$VHJhbnNoaXBtZW50=Transhipment");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "ListOfStrings"), "l1=One", "l2=Two");
			AssertResults(finder.FindStringUsages(typeof(TestClass).FullName, "MethodWithExceptionFilter"), "x1=Ok", "x2=Fail");
		}

		void AssertResults(CodeStringFinder.ResourceStringReference[] actualResults, params string[] expectedResults)
		{
			AssertContainsExactElementsInAnyOrder(expectedResults, Array.ConvertAll(actualResults, a => (a.Key ?? "") + "=" + (a.Value ?? "")));
		}

		[WTG.StaticAnalysis.Annotation.CodeAlive("Subclasses for test of code that anlyzes code")]
		class TestClass
		{
			public string NoResStringProperty
			{
				get { return "XXX"; }
			}

			public string ResStringProperty
			{
				get { return Res.GetString("T", "Test"); }
			}

			public string UnderScoreResStringProperty
			{
				get { return Res._GetString(0, "T", "UnderScore_Test"); }
			}

			public string ResStringWithParamters(string a, string b)
			{
				return Res.GetString("T", "String {0} with paramter {1}", a, b);
			}

			public virtual ZString ResStringWithIfLocalVariable
			{
				get
				{
					ZString result;
					if (i < 3)
					{
						if (i == 0)
						{
							result = Res.GetString("T0", "Zero");
						}
						else if (i == 1)
						{
							result = Res.GetString("T1", "One");
						}
						else
						{
							result = Res.GetString("T2", "Two");
						}
					}
					else
					{
						result = Res.GetString("T", "Other");
					}
					return result;
				}
			}

			public ZString ResStringWithSwitch
			{
				get
				{
					switch (Env.CurrentCompany.Country.Code)
					{
						case Constants.CountryCodes.Australia:
							return Res.GetString("AU", "Australia");
						case Constants.CountryCodes.UnitedStates:
							return Res.GetString("US", "United States");
						case Constants.CountryCodes.UnitedKingdom:
							return Res.GetString("UK", "United Kingdom");
						default:
							return ZString.Empty;
					}
				}
			}

			public string SelfRecursiveFunction(int i)
			{
				return Res.GetString("T", "Test {0}", i) + "\r\n" + (i > 0 ? SelfRecursiveFunction(i - 1) : "");
			}

			public string OutParameter
			{
				get
				{
					string x;
					if (new Dictionary<string, string>().TryGetValue("x", out x))
					{
						x = "?";
					}
					return x;
				}
			}

			public ZString ResStringFromRegistry
			{
				get { return DataRegistry.Instance.SignOffText; }
			}

			public string ResStringEmbedding
			{
				get { return Res.GetString("A", "The number is {0}", i >= 0 ? Res.GetString("P", "Positive") : Res.GetString("N", "Negative")); }
			}

			public string ResStringParameter
			{
				get { return Res.GetString("A", "The number is {0}", Number); }
			}

			public string Number
			{
				get { return i.ToString(); }
			}

			public string ResStringFromOverride
			{
				get { return VirtualResString; }
			}

			public virtual string VirtualResString { get { return string.Empty; } }
			public virtual string VirtualResString2 { get { return string.Empty; } }

			public MultilingualString MultilingualString
			{
				get { return ResString.GetMultilingualString("M", "Multilingual"); }
			}

			public MultilingualString UnderScoreMultilingualString
			{
				get { return ResString._GetMultilingualString(0, "M", "UnderScore_Multilingual"); }
			}

			public string GetDataString
			{
				get { return Res.GetData("X", string.Empty).FullDescription; }
			}

			public string GetCaculatedKeyString(string x)
			{
				return Res.GetString("X|" + x, "Test");
			}

			public CodeDescriptionPairList SalesModeList
			{
				get { return new CodeDescriptionPairList(OLookUpEditType.SalesMode); }
			}

			public AutoDayOfWeekCodeList DayOfWeekCodeList
			{
				get { return new AutoDayOfWeekCodeList(); }
			}

			public CodeDescriptionPairList FactoryCachedValue
			{
				get { return new BusinessObjectFactory().GetCachedValue<AutoDayOfWeekCodeList>(); }
			}

			public CodeDescriptionPairList CachedCodeDescriptionPairList
			{
				get { return new BusinessObjectFactory().GetCachedCodeDescriptionPairList(OLookUpEditType.SalesMode); }
			}

			public CodeDescriptionPairList FactoryCachedValueWithDelegate
			{
				get
				{
					return new BusinessObjectFactory().GetCachedValue("0515D681-0E8B-4C84-9B66-BB923A7C205C",
						delegate
						{
							var result = new CodeDescriptionPairList();
							result.AddPair("A", ResString.GetMultilingualString("A", "Alpha"));
							result.AddPair("B", ResString.GetMultilingualString("B", "Beta"));
							return result;
						});
				}
			}

			public CodeDescriptionPairList AnotherFactoryCachedValueWithDelegate
			{
				get
				{
					return new BusinessObjectFactory().GetCachedValue("0515D681-0E8B-4C84-9B66-BB923A7C205C",
						delegate
						{
							var result = new CodeDescriptionPairList();
							result.AddPair("1", ResString.GetMultilingualString("1", "one"));
							result.AddPair("2", ResString.GetMultilingualString("2", "two"));
							return result;
						});
				}
			}

			public CodeDescriptionPairList DelegateWithState(int state)
			{
				return new BusinessObjectFactory().GetCachedValue("0515D681-0E8B-4C84-9B66-BB923A7C205C",
					delegate
					{
						var result = new CodeDescriptionPairList();
						if (state == 3)
						{
							result.AddPair("3", ResString.GetMultilingualString("3", "three"));
						}
						else if (state == 4)
						{
							result.AddPair("4", ResString.GetMultilingualString("4", "four"));
						}
						return result;
					});
			}

			public string DescriptionFromListWithFunctionInConstructor
			{
				get { return new ListWithFunctionInConstructor()[i].Description; }
			}

			public string DescriptionFromDerivedList
			{
				get { return new DerivedList()[i].Description; }
			}

			[ReturnsResourceString]
			public object ReturnsResourceString()
			{
				return Res.GetString("T", "Test");
			}

			public string NeedHelp
			{
				[CodeStringFinderHint(typeof(TestHelperClass), nameof(TestHelperClass.Help))]
				get;
				set;
			}

			public string StringFromWrapper
			{
				get { return Wrapper.ToString(); }
			}

			StringWrapper Wrapper
			{
				get { return new StringWrapper(Res.GetString("W", "Wrapped")); }
			}

			public string StringFromStaticConstructor
			{
				get { return ClassWithStaticConstructor.Item.Name; }
			}

			public ZString TranslatableField(RefServiceLevel serviceLevel)
			{
				return serviceLevel.RS_DescriptionMultilingual;
			}

			public IEnumerable<ZString> ListOfStrings()
			{
				return new ZString[] { Res.GetString("l1", "One"), Res.GetString("l2", "Two") };
			}

			public string MethodWithExceptionFilter()
			{
				try
				{
					using (var client = new HttpClient())
					{
						return Res.GetString("x1", "Ok");
					}
				}
				catch (Exception ex) when (!ex.IsCriticalException())
				{
					return Res.GetString("x2", "Fail");
				}
			}

			public int i;

			public class InnerClass
			{
				public string ResString
				{
					get { return Res.GetString("I", "Inner"); }
				}
			}

			class ListWithFunctionInConstructor : CodeDescriptionPairList
			{
				public ListWithFunctionInConstructor()
				{
					Load();
				}

				void Load()
				{
					AddPair("A", ResString.GetMultilingualString("A", "Apple"));
					AddPair("B", ResString.GetMultilingualString("B", "Banana"));
				}
			}

			class DerivedList : ListWithFunctionInConstructor
			{
				public DerivedList()
					: base()
				{ }
			}

			[CodeStringFinderSupportedReturnType]
			class StringWrapper
			{
				public StringWrapper(string s)
				{
					this.s = s;
				}

				readonly string s;

				public override string ToString()
				{
					return s;
				}
			}
		}

		class TestClass2 : TestClass
		{
			public override ZString ResStringWithIfLocalVariable
			{
				get
				{
					if (i >= 0)
					{
						return base.ResStringWithIfLocalVariable;
					}
					return Res.GetString("N", "Negative");
				}
			}

			public override string VirtualResString
			{
				get { return Res.GetString("O", "Override"); }
			}
		}

		class TestClass3 : TestClass2
		{
			public override string VirtualResString2
			{
				get { return Res.GetString("O2", "Override after skipped override"); }
			}
		}

		class TestClass4
		{
			class TestClass5 : TestClass
			{
				public override string VirtualResString
				{
					get { return Res.GetString("O3", "Override from nested class"); }
				}
			}
		}

		class TestHelperClass
		{
			internal static void Help(TestClass data)
			{
				if (string.IsNullOrEmpty(data.NeedHelp))
				{
					data.NeedHelp = Res.GetString("H", "Helped");
				}
			}
		}

		[SuppressMessage("Enterprise.Globalization", "EDI001:ResourceStringStaticReferenceRule")]
		class ClassWithStaticConstructor
		{
			ClassWithStaticConstructor(string name)
			{
				this.Name = name;
			}

			public static readonly ClassWithStaticConstructor Item = new ClassWithStaticConstructor(Res.GetString("S", "Static"));

			public readonly string Name;
		}
	}
}
