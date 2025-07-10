using System;
using System.Linq.Expressions;
using CargoWise.EntityFramework;
using CargoWise.Organizations.CodeGeneration;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Business.Update.OrgCodeGenerations
{
	public class NativeOrgCodeGeneratorTest : TransactionedTestCase
	{
		public void TestGenerate()
		{
			orgMock.Setup(o => o.OH_FullName).Returns("ZZZZZZZZZ");
			orgMock.Setup(o => o.UnlocoCode).Returns("AUSYD");

			var orgCode = Generate();
			AssertNotEquals(string.Empty, orgCode);
		}

		public void TestGenerate_UNLOCO_Is_Empty()
		{
			orgMock.Setup(o => o.OH_FullName).Returns("ZZZZZZZZZ");
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException), () => { Generate(); });
		}

		public void TestGenerate_FullName_Is_Empty()
		{
			orgMock.Setup(o => o.UnlocoCode).Returns("AUSYD");
			AssertExceptionThrown(typeof(NativeXMLUserVisibleException), () => { Generate(); });
		}

		protected override void SetUp()
		{
			base.SetUp();
			generator = new NativeOrgCodeGenerator();
			orgMock = new Mock<IOrgCodeInfo>();
			factory = new BusinessObjectFactory();
		}

		string Generate()
		{
			MakeNullStringsOnOrgEmpty();
			return generator.Generate(orgMock.Object, factory);
		}

		void MakeNullStringsOnOrgEmpty()
		{
			foreach (var property in typeof(IOrgCodeInfo).GetProperties())
			{
				if (property.PropertyType == typeof(string))
				{
					if (property.GetValue(orgMock.Object, null) == null)
					{
						var parameterExp = Expression.Parameter(typeof(IOrgCodeInfo));
						var propertyExp = Expression.Property(parameterExp, property);
						var getter = Expression.Lambda<Func<IOrgCodeInfo, string>>(propertyExp, parameterExp);

						orgMock.Setup(getter).Returns(string.Empty);
					}
				}
			}
		}

		NativeOrgCodeGenerator generator;
		Mock<IOrgCodeInfo> orgMock;
		BusinessObjectFactory factory;
	}
}
