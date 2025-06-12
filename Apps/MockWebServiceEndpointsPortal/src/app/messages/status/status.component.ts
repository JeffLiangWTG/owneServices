import { Component, Input } from '@angular/core';
import * as model from '../../model/messaging-interface'

@Component({
	selector: 'app-status',
	templateUrl: './status.component.html'
})

export class AppStatusComponent {
	@Input() status: model.AppSatus = {
		busy: false,
		message: ''
	}
}