using System;
using System.Collections.Generic;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using CargoWise.Macros;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business
{
	sealed class DescriptionMacroLibrary : MacroLibrary
	{
		internal DescriptionMacroLibrary(BusinessObjectFactory factory)
		{
			this.factory = factory;
		}

		readonly BusinessObjectFactory factory;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetStringAnalyzer", Justification = "This is a macro name, This is a macro description")]
		protected override IEnumerable<IHandler> MacroHandlers
		{
			get
			{
				yield return new Handler<Func<string, string, string>>(
					"Description",
					"Returns the description of the entity identified by its primary key and table prefix",
					(pk, tablePrefix) => GetDescription(pk, tablePrefix)
					);
				yield return new Handler<Func<string, string, string>>(
					"DescriptionFromCode",
					"Returns the description of the entity identified by its code and table prefix",
					(code, tablePrefix) => GetDescriptionFromCode(code, tablePrefix)
					);
				yield return new Handler<Func<string, string>>(
					"AddressDescriptionFromCode",
					"Returns the description of the address by its code",
					(code) => AddressCodeDescriptionHelper.GetAddressDescriptionFromCode(code)
					);
			}
		}

		string GetDescription(string pk, string tablePrefix)
		{
			Guid guid;

			if (Guid.TryParse(pk, out guid))
			{
				var bizo = factory.Load(tablePrefix, guid) as ICodeDescription;

				return bizo?.Description;
			}
			else
			{
				return null;
			}
		}

		string GetDescriptionFromCode(string code, string tablePrefix)
		{
			var schema = EnterpriseSchema.GetTableSchemaFromColumnNamePrefix(tablePrefix);
			if (schema == null)
			{ return null; }
			var type = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(tablePrefix);
			if (type == null)
			{ return null; }
			var codeProperty = CodePropertyAttribute.CodePropertyNameFromType(type);
			if (codeProperty == null)
			{ return null; }
			var bizO = factory.LoadTop1(type, new ZQuery(schema.GetSchemaColumn(codeProperty), code)) as ICodeDescription;
			return bizO?.Description;
		}

		IAddressCodeDescriptionHelper AddressCodeDescriptionHelper
		{
			get { return factory.GetCachedValue("StmALog_AddressCodeDescriptionHelper", () => ObjectFactory.Get<IAddressCodeDescriptionHelper>(nameof(IAddressCodeDescriptionHelper), new object[] { factory })); }
		}
	}
}
