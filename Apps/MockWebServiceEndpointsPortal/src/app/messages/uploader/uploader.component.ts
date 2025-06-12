import { Component, Input } from '@angular/core';
import * as model from '../../model/messaging-interface'
import { MessagesManager } from '../../shared/messages.service'

@Component({
	selector: 'message-uploader',
	templateUrl: './uploader.component.html'
})

export class UploaderComponent {
	@Input() callback: Function | undefined;
	@Input() messageBoxStatus: model.AppSatus | undefined
	files: File[] = [];
	hasFiles: boolean = false;

	constructor(private messageManager:MessagesManager) {
	}

	upload(event: any) {
		if (this.files) {
			// call upload messages
			this.messageManager.upload(Array.from(this.files), () => {
				if (this.callback) this.callback();
			}).then(data => {
				this.files = [];
				this.hasFiles = false;
				event.target.files = undefined;
			});
		}
	}

	cancel() {
		this.files = [];
		this.hasFiles = false;
	}

	handleFileInput(event: any) {
		this.files = event.target.files as File[];
		this.hasFiles = this.files && this.files.length !== 0;
		console.log(this.files);
	}
}