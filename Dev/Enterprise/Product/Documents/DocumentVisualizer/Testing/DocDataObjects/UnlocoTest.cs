using System;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentVisualizer.Core;
using Enterprise.DocumentVisualizer.DocDataObjects;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentVisualizer.Testing
{
	[TestedType(typeof(Unloco))]
	sealed class UnlocoTest : NonPersistentBusinessObjectTestCase
	{
		#region TestCreate

		public void TestCreate_FromRefUnloco()
		{
			var ausyd = new RefUNLOCO.Loader(Factory).Load("AUSYD");

			var unloco = Unloco.Create(Context, ausyd);
			AssertEquals(nameof(unloco.Code), "AUSYD", unloco.Code);
			AssertEquals(nameof(unloco.Name), "Sydney", unloco.Name);
			AssertEquals("Country.Code", "AU", unloco.Country.Code);
			AssertEquals("Country.Name", "Australia", unloco.Country.Name);
		}

		public void TestCreate_FromUnloco()
		{
			var ausyd = new RefUNLOCO.Loader(Factory).Load("AUSYD");

			var unloco = Unloco.Create(Context, ausyd);
			var otherUnloco = Unloco.Create(Context, unloco);

			AssertEquals(nameof(unloco.Code), "AUSYD", otherUnloco.Code);
			AssertEquals(nameof(unloco.Name), "Sydney", otherUnloco.Name);
			AssertEquals("Country.Code", "AU", otherUnloco.Country.Code);
			AssertEquals("Country.Name", "Australia", otherUnloco.Country.Name);
		}

		public void TestCreateRefUnloco_CodeDisableModifiable()
		{
			var ausyd = new RefUNLOCO.Loader(Factory).Load("AUSYD");

			var unloco = Unloco.Create(Context, ausyd, false);
			AssertEquals(nameof(unloco.Code), "AUSYD", unloco.Code);
			AssertEquals(nameof(unloco.Name), "Sydney", unloco.Name);
			AssertEquals("Country.Code", "AU", unloco.Country.Code);
			AssertEquals("Country.Name", "Australia", unloco.Country.Name);
			Assert("Code enabled modifiable", !unloco.Code_DisableModifiable);

			unloco = Unloco.Create(Context, ausyd, true);
			Assert("Code disabled modifiable", unloco.Code_DisableModifiable);

			var propertyInfo = typeof(Unloco).GetProperty(nameof(unloco.Code));
			var attribute = (DisableModifiableMemberAttribute)propertyInfo?.GetCustomAttribute(typeof(DisableModifiableMemberAttribute));
			AssertNotNull("Code does not have DisableModifiableMemberAttribute applied", attribute);
			AssertEquals("The member name to be Code_DisableModifiable", "Code_DisableModifiable", attribute?.Member);
		}

		public void TestCreate_WithCodeMapper()
		{
			var foreignCodeMap = new Dictionary<string, string>
			{
				["AUSYD"] = "AUAAA",
				["AUMEL"] = "AUBBB",
			};

			var localCodeMap = new Dictionary<string, string>
			{
				["AUAAA"] = "AUSYD",
				["AUBBB"] = "AUMEL"
			};

			var unlocosWithMap = new RefUNLOCOCollectionWithMap(foreignCodeMap, localCodeMap);
			unlocosWithMap.ShowForeignCode = true;

			var context = new Mock<IContext>();
			context.SetupGet(c => c.Factory).Returns(Factory);
			context.SetupGet(c => c.Unlocos).Returns(unlocosWithMap);
			context.SetupGet(c => c.Countries).Returns(new RefCountryCollection(Factory));

			var ausyd = new RefUNLOCO.Loader(Factory).Load("AUSYD");

			var unloco = Unloco.Create(context.Object, ausyd);
			AssertEquals("AUSYD code was mapped to local code for unloco created from a RefUnloco", "AUAAA", unloco.Code);
			AssertEquals("Name for mapped unloco", "Sydney", unloco.Name);
			AssertEquals("Country.Code for mapped unloco", "AU", unloco.Country.Code);
			AssertEquals("Country.Name for mapped unloco", "Australia", unloco.Country.Name);

			var otherUnloco = Unloco.Create(context.Object, unloco);
			AssertEquals("AUSYD code was mapped to local code for unloco created from another Unloco", "AUAAA", otherUnloco.Code);
			AssertEquals("Name for mapped otherUnloco", "Sydney", otherUnloco.Name);
			AssertEquals("Country.Code for mapped otherUnloco", "AU", otherUnloco.Country.Code);
			AssertEquals("Country.Name for mapped otherUnloco", "Australia", otherUnloco.Country.Name);
		}

		#endregion

		#region TestCodeWithMapper

		public void TestCodeWithMapper_ShowForeignCode()
		{
			var foreignCodeMap = new Dictionary<string, string>
			{
				["AUSYD"] = "AUAAA",
				["NZAKL"] = "NZAAA",
				["PLWRO"] = "ZZAAA",
				["USCHI"] = "USNHX"
			};

			var localCodeMap = new Dictionary<string, string>
			{
				["AUAAA"] = "AUSYD",
				["NZAAA"] = "NZAKL",
				["ZZAAA"] = "PLWRO",
				["USNHX"] = "USCHI"
			};

			var unlocosWithMap = new RefUNLOCOCollectionWithMap(foreignCodeMap, localCodeMap);
			unlocosWithMap.ShowForeignCode = true;

			var context = new Mock<IContext>();
			context.SetupGet(c => c.Factory).Returns(Factory);
			context.SetupGet(c => c.Unlocos).Returns(unlocosWithMap);
			context.SetupGet(c => c.Countries).Returns(new RefCountryCollection(Factory));

			var ausyd = new RefUNLOCO.Loader(Factory).Load("AUSYD");

			var unloco = Unloco.Create(context.Object, ausyd);
			AssertEquals("prerequisite; AUSYD code was mapped to local code", "AUAAA", unloco.Code);

			unloco.Code = "NZAAA";
			AssertEquals("mainained mapped foreign code", "NZAAA", unloco.Code);
			AssertEquals("mainained mapped name", "Auckland", unloco.Name);
			AssertEquals("set local mapped country code", "NZ", unloco.Country.Code);
			AssertEquals("set local mapped country name ", "New Zealand", unloco.Country.Name);

			unloco.Code = "SGSIN";
			AssertEquals("mainained unmapped code", "SGSIN", unloco.Code);
			AssertEquals("set local unmapped name", "Singapore", unloco.Name);
			AssertEquals("set local unmapped country code", "SG", unloco.Country.Code);
			AssertEquals("set local unmapped country name ", "Singapore", unloco.Country.Name);

			unloco.Code = "ZZAAA";
			AssertEquals("mainained mapped foreign code", "ZZAAA", unloco.Code);
			AssertEquals("set local mapped name", "Wroclaw", unloco.Name);
			AssertEquals("set local mapped country code", "PL", unloco.Country.Code);
			AssertEquals("set local mapped country name ", "Poland", unloco.Country.Name);

			unloco.Code = "USCHI";
			AssertEquals("mainained mapped foreign code", "USNHX", unloco.Code);
			AssertEquals("set local mapped name", "Foley", unloco.Name);
			AssertEquals("set local mapped country code", "US", unloco.Country.Code);
			AssertEquals("set local mapped country name ", "United States", unloco.Country.Name);
		}

		public void TestCodeWithMapper_ShowLocalCode()
		{
			var foreignCodeMap = new Dictionary<string, string>
			{
				["AUSYD"] = "AUAAA",
				["NZAKL"] = "NZAAA",
				["PLWRO"] = "ZZAAA",
				["USCHI"] = "USNHX"
			};

			var localCodeMap = new Dictionary<string, string>
			{
				["AUAAA"] = "AUSYD",
				["NZAAA"] = "NZAKL",
				["ZZAAA"] = "PLWRO",
				["USNHX"] = "USCHI"
			};

			var unlocosWithMap = new RefUNLOCOCollectionWithMap(foreignCodeMap, localCodeMap);
			unlocosWithMap.ShowForeignCode = false;

			var context = new Mock<IContext>();
			context.SetupGet(c => c.Factory).Returns(Factory);
			context.SetupGet(c => c.Unlocos).Returns(unlocosWithMap);
			context.SetupGet(c => c.Countries).Returns(new RefCountryCollection(Factory));

			var ausyd = new RefUNLOCO.Loader(Factory).Load("AUSYD");

			var unloco = Unloco.Create(context.Object, ausyd);
			AssertEquals("prerequisite; AUSYD code was mapped to local code", "AUSYD", unloco.Code);

			unloco.Code = "NZAAA";
			AssertEquals("mainained mapped local code", "NZAKL", unloco.Code);
			AssertEquals("mainained mapped name", "Auckland", unloco.Name);
			AssertEquals("set local mapped country code", "NZ", unloco.Country.Code);
			AssertEquals("set local mapped country name ", "New Zealand", unloco.Country.Name);

			unloco.Code = "SGSIN";
			AssertEquals("mainained unmapped code", "SGSIN", unloco.Code);
			AssertEquals("set local unmapped name", "Singapore", unloco.Name);
			AssertEquals("set local unmapped country code", "SG", unloco.Country.Code);
			AssertEquals("set local unmapped country name ", "Singapore", unloco.Country.Name);

			unloco.Code = "ZZAAA";
			AssertEquals("mainained mapped local code", "PLWRO", unloco.Code);
			AssertEquals("set local mapped name", "Wroclaw", unloco.Name);
			AssertEquals("set local mapped country code", "PL", unloco.Country.Code);
			AssertEquals("set local mapped country name ", "Poland", unloco.Country.Name);

			unloco.Code = "USCHI";
			AssertEquals("mainained mapped local code", "USCHI", unloco.Code);
			AssertEquals("set local mapped name", "Chicago", unloco.Name);
			AssertEquals("set local mapped country code", "US", unloco.Country.Code);
			AssertEquals("set local mapped country name ", "United States", unloco.Country.Name);
		}

		#endregion

		#region TestCodeConvertsToUppercase

		public void TestCodeConvertsToUppercase()
		{
			var unloco = new Unloco(Factory, Context.Unlocos, Context.Countries);
			unloco.Code = "ausyd";

			AssertEquals("Code", "AUSYD", unloco.Code);

			unloco.IATACode = "asy";
			AssertEquals("IATACode", "ASY", unloco.IATACode);

			unloco.Code = "SGsin";
			AssertEquals("Code", "SGSIN", unloco.Code);

			unloco.IATACode = "aSy";
			AssertEquals("IATACode", "ASY", unloco.IATACode);

			unloco.Code = "PLWRO";
			AssertEquals("Code", "PLWRO", unloco.Code);

			unloco.IATACode = "ASY";
			AssertEquals("IATACode", "ASY", unloco.IATACode);
		}

		#endregion

		#region TestUpdateName

		public void TestUpdateName()
		{
			var unloco = new Unloco(Factory, Context.Unlocos, Context.Countries);

			unloco.Code = "PLWRO";
			AssertEquals("Name", "Wroclaw", unloco.Name);

			unloco.Code = "PLGDN";
			AssertEquals("Name", "Gdansk", unloco.Name);
		}

		#endregion

		#region TestCustomNameProvider

		public void TestCustomNameProvider()
		{
			string GetName(IRefUNLOCO refUnloco) => $"{refUnloco?.RL_PortName} test".Trim();

			var unloco = new Unloco(Factory, Context.Unlocos, Context.Countries).WithCustomNameProvider(GetName);

			unloco.Code = "PLWRO";
			AssertEquals("Name", "Wroclaw test", unloco.Name);

			unloco.Code = "PLGDN";
			AssertEquals("Name", "Gdansk test", unloco.Name);

			unloco.Code = "XXXX";
			AssertEquals("Name", "test", unloco.Name);
		}

		#endregion

		#region TestUpdateCountry

		public void TestUpdateCountry()
		{
			var unloco = new Unloco(Factory, Context.Unlocos, Context.Countries);

			unloco.Code = "PLWRO";
			AssertEquals("Country.Code", "PL", unloco.Country.Code);
			AssertEquals("Country.Name", "Poland", unloco.Country.Name);

			unloco.Code = "AUSYD";
			AssertEquals("Country.Code", "AU", unloco.Country.Code);
			AssertEquals("Country.Name", "Australia", unloco.Country.Name);
		}

		#endregion

		#region TestUpdateIATACode

		public void TestUpdateIATACode()
		{
			var unloco = new Unloco(Factory, Context.Unlocos, Context.Countries);

			unloco.IATACode = "GDN";
			AssertEquals("IATACode", "GDN", unloco.IATACode);

			unloco.IATACode = "SYD";
			AssertEquals("IATACode", "SYD", unloco.IATACode);
		}

		#endregion

		#region TestToString

		public void TestToString()
		{
			var unloco = new Unloco(Factory, Context.Unlocos, Context.Countries);

			unloco.Code = "PLWRO";

			AssertEquals("ToString", "PLWRO - Wroclaw", unloco.ToString());
		}

		#endregion

		#region TestOverriddenUnlocoResetsCorrectly

		public void TestOverriddenUnlocoResetsCorrectly()
		{
			var unloco = new Unloco(Factory, Context.Unlocos, Context.Countries);
			unloco.Code = "AUSYD";

			AssertEquals("prereq: Name has been updated from Code", unloco.Name, "Sydney");

			var dynamicUnloco = unloco.MakeDocDataDynamic();

			const string xml =
@"<Entity>
	<Property Name=""Code"">
		<Value>AUFRE</Value>
	</Property>
	<Property Name=""Name"">
		<Value>Fremantle</Value>
	</Property>
</Entity>";

			var xmlDoc = XDocument.Parse(xml);

			dynamicUnloco.ApplyDataFromXml(xmlDoc);

			var code = dynamicUnloco.GetDynamicProperty(nameof(unloco.Code));
			var name = dynamicUnloco.GetDynamicProperty(nameof(unloco.Name));

			AssertEquals("Code override applied", "AUFRE", code.Value);
			AssertEquals("Name override applied", "Fremantle", name.Value);

			Assert("Code is marked as overridden", code.IsOverridden);
			Assert("Name is marked as overridden", name.IsOverridden);

			Assert("Code is marked as not having changes", !code.HasChanges);
			Assert("Name is marked as not having changes", !name.HasChanges);

			dynamicUnloco.CancelChanges();

			AssertEquals("Code has been reset correctly", "AUSYD", code.Value);
			AssertEquals("Name has been reset correctly", "Sydney", name.Value);

			Assert("Code after reset is marked as having changes", code.HasChanges);
			Assert("Name after reset is marked as having changes", name.HasChanges);
		}

		#endregion

		#region Implementation

		sealed class RefUNLOCOCollectionWithMap : IRefUNLOCOCollection, ICodeMapper
		{
			public RefUNLOCOCollectionWithMap(IReadOnlyDictionary<string, string> foreignCodeMap, IReadOnlyDictionary<string, string> localCodeMap)
			{
				this.foreignCodeMap = foreignCodeMap ?? throw new ArgumentNullException(nameof(foreignCodeMap));
				this.localCodeMap = localCodeMap ?? throw new ArgumentNullException(nameof(localCodeMap));
			}

			readonly IReadOnlyDictionary<string, string> foreignCodeMap;
			readonly IReadOnlyDictionary<string, string> localCodeMap;

			public bool ShowForeignCode { get; set; }

			string ICodeMapper.GetForeignCode(string localCode)
			{
				if (foreignCodeMap.TryGetValue(localCode, out var res))
				{
					return res;
				}

				return localCode;
			}

			string ICodeMapper.GetLocalCode(string foreignCode)
			{
				if (localCodeMap.TryGetValue(foreignCode, out var res))
				{
					return res;
				}

				return foreignCode;
			}
		}

		IContext Context
		{
			get
			{
				if (context == null)
				{
					var mock = new Mock<IContext>();
					mock.SetupGet(c => c.Factory).Returns(Factory);
					mock.SetupGet(c => c.Unlocos).Returns(new RefUNLOCOCollection(Factory));
					mock.SetupGet(c => c.Countries).Returns(new RefCountryCollection(Factory));

					context = mock.Object;
				}

				return context;
			}
		}
		IContext context;

		protected override BusinessObject GetNewBusinessObject()
		{
			var ausyd = new RefUNLOCO.Loader(Factory).Load("AUSYD");
			return Unloco.Create(Context, ausyd);
		}

		#endregion
	}
}
