using System;
using System.Collections.Generic;
using Enterprise.DataTransfer.Native.Common.EntityRepositories;
using Enterprise.DataTransfer.Native.Common.Operations;
using Moq;
using NUnit.Framework;

namespace Enterprise.DataTransfer.Native.Common
{
	public class UpdateOperationTest : TransactionedTestCase
	{
		[ExpectNoExceptions]
		public void TestUpdate()
		{
			updateOperation.Update(entitySet.Object);

			entityRepository.Verify(er => er.OpenSession(), Times.Once);
			entityRepository.Verify(er => er.CloseSession(), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestUpdateAction_Insert()
		{
			entity.Setup(e => e.Action).Returns(EntityAction.INSERT);
			entityRepository.Setup(er => er.Insert(entity.Object)).Returns(entity.Object);

			updateOperation.UpdateAction(entity.Object, null);

			entityRepository.Verify(er => er.Insert(entity.Object), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestUpdateAction_Update()
		{
			entity.Setup(e => e.Action).Returns(EntityAction.UPDATE);
			entityRepository.Setup(er => er.Update(entity.Object)).Returns(entity.Object);

			updateOperation.UpdateAction(entity.Object, null);

			entityRepository.Verify(er => er.Update(entity.Object), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestUpdateAction_Update_WhenUpdateRefCountry()
		{
			entity.Setup(e => e.Action).Returns(EntityAction.UPDATE);
			entity.Setup(e => e.TableName).Returns("RefCountry");

			updateOperation.UpdateAction(entity.Object, null);

			entityRepository.Verify(er => er.Update(entity.Object), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestUpdateAction_Merge_RecordFound()
		{
			entity.Setup(e => e.Action).Returns(EntityAction.MERGE);

			entityRepository.Setup(er => er.Update(entity.Object)).Returns(entity.Object);

			updateOperation.UpdateAction(entity.Object, null);

			entityRepository.Verify(er => er.Update(entity.Object), Times.Once);
			entityRepository.Verify(er => er.Insert(entity.Object), Times.Never);
		}

		[ExpectNoExceptions]
		public void TestUpdateAction_Merge_RecordNotFound()
		{
			entity.Setup(e => e.Action).Returns(EntityAction.MERGE);

			entityRepository.SetupSequence(er => er.Update(entity.Object)).Throws(new InvalidOperationException());
			entityRepository.Setup(er => er.Insert(entity.Object)).Returns(entity.Object);

			updateOperation.UpdateAction(entity.Object, null);

			entityRepository.Verify(er => er.Update(entity.Object), Times.Once);
			entityRepository.Verify(er => er.Insert(entity.Object), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestUpdateAction_Merge_WhenUpdateRefCountry()
		{
			entity.Setup(e => e.Action).Returns(EntityAction.MERGE);
			entity.Setup(e => e.TableName).Returns("RefCountry");

			updateOperation.UpdateAction(entity.Object, null);

			entityRepository.Verify(er => er.Update(entity.Object), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestUpdateAction_Delete()
		{
			entity.Setup(e => e.Action).Returns(EntityAction.DELETE);
			entityRepository.Setup(er => er.Delete(entity.Object)).Returns(entity.Object);

			updateOperation.UpdateAction(entity.Object, null);

			entityRepository.Verify(er => er.Delete(entity.Object), Times.Once);
		}

		[ExpectNoExceptions]
		public void TestUpdateAction_Delete_WhenUpdateRefCountry()
		{
			entity.Setup(e => e.Action).Returns(EntityAction.DELETE);

			updateOperation.UpdateAction(entity.Object, null);

			entityRepository.Verify(er => er.Delete(entity.Object), Times.Once);
			mocks.VerifyAll();
		}

		[ExpectNoExceptions]
		public void TestUpdateAction_Empty()
		{
			entity.Setup(e => e.Action).Returns(EntityAction.EMPTY);

			var repository = new MockRepository(MockBehavior.Strict);
			var iPropertyDef = repository.Create<IPropertyDef>();

			entity.Setup(e => e.Properties).Returns(new List<Property>() { new Property(iPropertyDef.Object) { Value = "AAAA" } });
			entityRepository.Setup(er => er.Find(entity.Object)).Returns(entity.Object);

			updateOperation.UpdateAction(entity.Object, null);

			entityRepository.Verify(er => er.Find(entity.Object), Times.Once);
			mocks.VerifyAll();
		}
		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			mocks = new MockRepository(MockBehavior.Loose);
			entityRepository = mocks.Create<IEntityRepository>();
			entitySet = mocks.Create<IEntitySet>();
			entity = mocks.Create<IEntity>();
			updateOperation = new UpdateOperation { EntityRepository = entityRepository.Object };
		}

		#endregion

		MockRepository mocks;
		Mock<IEntityRepository> entityRepository;
		UpdateOperation updateOperation;
		Mock<IEntitySet> entitySet;
		Mock<IEntity> entity;
	}
}
