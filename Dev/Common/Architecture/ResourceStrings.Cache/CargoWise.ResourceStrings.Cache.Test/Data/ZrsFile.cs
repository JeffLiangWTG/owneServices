#if NETFRAMEWORK // Disable for .NET Core until ResourceStrings are supported in .NET Core
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWiseOne.ResourceStrings;
using Mono.Cecil;
using NUnit.Framework;

namespace CargoWise.ResourceStrings.Cache
{
	public class ZrsFileTest : TestCase
	{
		public void TestCalculateAsmid()
		{
			CombineAssertions(delegate
			{
				unchecked
				{
					AssertEquals(differentItems[0], (ushort)ZrsFile.CalculateAsmid(differentItems[0]), GetAsmidAttributeValue(differentItems[0]));
					AssertEquals(differentItems[1], (ushort)ZrsFile.CalculateAsmid(differentItems[1]), GetAsmidAttributeValue(differentItems[1]));
					AssertEquals(differentItems[2], (ushort)ZrsFile.CalculateAsmid(differentItems[2]), GetAsmidAttributeValue(differentItems[2]));
					AssertEquals(differentItems[3], (ushort)ZrsFile.CalculateAsmid(differentItems[3]), GetAsmidAttributeValue(differentItems[3]));
					AssertEquals(differentItems[4], (ushort)ZrsFile.CalculateAsmid(differentItems[4]), GetAsmidAttributeValue(differentItems[4]));
					AssertEquals(differentItems[5], (ushort)ZrsFile.CalculateAsmid(differentItems[5]), GetAsmidAttributeValue(differentItems[5]));
					AssertEquals(differentItems[6], (ushort)ZrsFile.CalculateAsmid(differentItems[6]), GetAsmidAttributeValue(differentItems[6]));
				}
			});
		}

		public void TestHashRelationships()
		{
			unchecked
			{
				var hashes = new HashSet<ushort>();
				foreach (var item in differentItems)
				{
					ushort hash = (ushort)ZrsFile.CalculateAsmid(item);
					if (hashes.Contains(hash))
					{
						Fail("Two different items found with the same hash value");
					}
					else
					{
						hashes.Add(hash);
					}
				}

				AssertEquals((ushort)ZrsFile.CalculateAsmid("wc.Enterprise.ZArchitecture.Web.GUI"), (ushort)ZrsFile.CalculateAsmid("wc.Enterprise.ZArchitecture.Web.GUI"));
			}
		}

		ushort GetAsmidAttributeValue(string assemblyName)
		{
			ushort assemblyId = 0;
			CustomAttribute assemblyIdAttribute = null;
			var assemblyDefinition = AssemblyDefinition.ReadAssembly(Path.Combine(BinPath, assemblyName + ".dll"));
			foreach (var attribute in assemblyDefinition.CustomAttributes)
			{
				if (attribute.AttributeType.FullName == typeof(ResourceStringAssemblyIdAttribute).FullName)
				{
					assemblyIdAttribute = attribute;
					break;
				}
			}
			if (assemblyIdAttribute != null)
			{
				assemblyId = (ushort)assemblyIdAttribute.ConstructorArguments[0].Value;
			}
			return assemblyId;
		}

		string BinPath
		{
			get { return binPath ?? (binPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)); }
		}
		string binPath;

		readonly string[] differentItems = new string[]
			{
				"Enterprise.ZArchitecture.GUI",
				"Enterprise.DocumentEngine",
				"Enterprise.DocumentEngine.GUI",
				"Enterprise.MasterFiles.Business",
				"Enterprise.MasterFiles.GUI",
				"Enterprise.Accounting.Business",
				"Enterprise.Warehouse.Transactions.Business"
			};
	}
}
#endif
